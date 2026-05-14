# Phase 1 Data Model

**Date**: 2026-05-14
**Status**: Final
**Scope**: Only entities, DbContext mappings, and response DTOs added or modified by this feature. Entities not listed here are reused as-is.

---

## Entities (no schema-level field changes)

The feature deliberately introduces *no new entities* and *no new entity fields*. All changes are at the persistence layer (indexes) and at the read-side (paginated queries return existing columns).

| Entity | Module | New attributes? | Behavioural change? |
|---|---|---|---|
| `ManagementGroup` | ManagementGroups | None | Handlers add uniqueness pre-check, but entity itself unchanged. |
| `GroupMember` | ManagementGroups | None | None. |
| `UserReadModel` | ManagementGroups | None | Read-model already populated by `UserSignedUp`. |
| `ResourceType` | Resources | None | Handlers add uniqueness pre-check. |
| `ResourcePropertyDefinition` | Resources | None | None. |
| `ResourceInstance` | Resources | None | None. |
| `ResourcePropertyValue` | Resources | None | None. |
| `GroupReadModel` | Resources | None | None. |
| `GroupMemberReadModel` | Resources | None | None. |

---

## Database constraints (new)

### `management_groups.management_groups`

```sql
CREATE UNIQUE INDEX "IX_management_groups_OwnerId_Name_NotDeleted"
    ON management_groups.management_groups ("OwnerId", "Name")
    WHERE "DeletedAt" IS NULL;
```

Configured via:

```csharp
builder.HasIndex(g => new { g.OwnerId, g.Name })
       .IsUnique()
       .HasFilter("\"DeletedAt\" IS NULL");
```

### `resources.resource_types`

```sql
CREATE UNIQUE INDEX "IX_resource_types_GroupId_Name"
    ON resources.resource_types ("GroupId", "Name");
```

Configured via:

```csharp
builder.HasIndex(t => new { t.GroupId, t.Name }).IsUnique();
```

### `resources.resource_instances` — supporting index for cursor pagination

```sql
CREATE INDEX "IX_resource_instances_GroupId_Id"
    ON resources.resource_instances ("GroupId", "Id")
    WHERE "DeletedAt" IS NULL;
```

Composite index supports the cursor query `WHERE GroupId = @g AND Id > @after ORDER BY Id LIMIT @take` filtered to not-soft-deleted rows. Configured via:

```csharp
builder.HasIndex(i => new { i.GroupId, i.Id })
       .HasFilter("\"DeletedAt\" IS NULL");
```

---

## Result records introduced

### `IsGroupNameAvailableResult` (ManagementGroups)

```csharp
public sealed record IsGroupNameAvailableResult(bool Available);
```

Returned by `IsGroupNameAvailableQueryHandler`. `Available == true` ⇒ no other group owned by the caller has that name (case-insensitive trimmed comparison) AND the name is non-empty / within length limits.

### `GetGroupMembersResult` (ManagementGroups)

```csharp
public sealed record GetGroupMembersResult(
    IReadOnlyList<GroupMemberDto> Items,
    Guid? NextCursor);

public sealed record GroupMemberDto(
    Guid UserId,
    string Email,
    DateTimeOffset JoinedAt,
    bool IsOwner);
```

`Items` is ordered by `GroupMember.Id` ascending. `NextCursor` = `Items.Last().UserId` when `Items.Count == take`, else `null`. The owner is always returned in the very first page regardless of cursor; subsequent pages are member rows. Implementation note: the owner is synthesised in the handler from the group's `OwnerId` join'd with `UserReadModel`, then concatenated to the actual `GroupMember` rows; cursor is over the union sorted by `UserId`. This keeps the API uniform.

### `GetResourceInstancesResult` (Resources) — replaces existing flat list response

```csharp
public sealed record GetResourceInstancesResult(
    IReadOnlyList<ResourceInstanceDto> Items,
    Guid? NextCursor);
```

`ResourceInstanceDto` keeps its existing shape (no changes). Old callers that expected a flat array will break — accepted because the only caller is the FE client which gets regenerated.

---

## Result records modified

### `GetManagementGroupResult` — adds derived field

```csharp
public sealed record GetManagementGroupResult(
    Guid Id,
    string Name,
    string? Description,
    Guid OwnerId,
    DateTimeOffset CreatedAt,
    int MemberCount,           // NEW — computed
    int ResourceTypeCount,     // NEW — computed (cross-module via read-model? See note)
    int ResourceInstanceCount, // NEW — computed (cross-module via read-model? See note)
    IReadOnlyList<GroupMemberDto> Members);
```

**Cross-module note**: `ResourceTypeCount` and `ResourceInstanceCount` are owned by the Resources module. ManagementGroups MUST NOT query the Resources schema directly (constitution gate I). Two options:

1. **Defer counts to FE** — let the FE call `GET /management-groups/{id}` and `GET /resources/types?groupId=X` + `GET /resources/instances?groupId=X` separately and compute counts. Simpler, no new cross-module surface.
2. **Add a paginated tail signalling total** — also simpler than cross-module sync.

**Decision**: Option 1 — counts are FE-side. `GetManagementGroupResult` does **NOT** add ResourceTypeCount / ResourceInstanceCount fields. Spec section "Group Header" requests these counts as meta-chips; FE computes them from the existing API responses. This eliminates the need for an Outbox-published `ResourceTypeCount` read-model and avoids breaking the constitution gate.

**Revised** `GetManagementGroupResult`:

```csharp
public sealed record GetManagementGroupResult(
    Guid Id,
    string Name,
    string? Description,
    Guid OwnerId,
    DateTimeOffset CreatedAt,
    int MemberCount,           // NEW — owner inclusive, from same module
    IReadOnlyList<GroupMemberDto> Members);
```

`MemberCount` is computed by the handler from members count + 1 (owner). Returned eagerly because the Group Header chip needs it before infinite-scroll fetches members.

---

## State transitions

No new state machines introduced. Existing soft-delete on `ManagementGroup` and `ResourceInstance` continues to work.

The cascade triggered by `GroupDeleted` event:

```
GroupDeleted(GroupId)
  ├── Resources module:
  │     ├── For each ResourceInstance in group: set DeletedAt = now() (soft)
  │     └── For each ResourceType in group: DELETE row + cascaded property definitions
  └── (no other subscribers in this feature)
```

The cascade is idempotent — second delivery finds zero rows matching and no-ops.

---

## DTOs (Api layer)

### `IsGroupNameAvailableQuery` (no body — query string only)

Endpoint: `GET /management-groups/name-available?name=…`
Query parameters: `name: string` (required, length 1–100).
No request DTO record needed (single primitive parameter); the endpoint constructs `IsGroupNameAvailableQuery` directly from `[FromQuery] string name` and the JWT-derived caller id.

### `GetGroupMembersQuery` — constructed in endpoint

Endpoint: `GET /management-groups/{id}/members?afterId=…&take=…`
Endpoint constructs `GetGroupMembersQuery(GroupId: id, AfterId: afterId, Take: take ?? 20, CallerUserId: claimsId)`. Take clamped to `[1, 50]` in handler.

### Existing endpoints

`CreateManagementGroupRequest` / `UpdateManagementGroupRequest` unchanged (server-side uniqueness check is added behind the handler, not in the request DTO).

`CreateResourceTypeRequest` / `UpdateResourceTypeRequest` unchanged.

`GetResourceInstancesQuery` adds optional `AfterId` and `Take` properties:

```csharp
public sealed record GetResourceInstancesQuery(
    Guid CallerUserId,
    Guid? GroupId,
    Guid? ResourceTypeId,
    bool IncludeDeleted,
    Guid? AfterId,
    int Take);
```

---

## Validation summary (per requirement)

| Rule | Where enforced | Source spec FR |
|---|---|---|
| Group name 1–100 chars | Reactive Form (FE) + handler validation (BE) | FR-001 |
| Group name unique per owner (case-insensitive) | Async client check (FE) + handler pre-check (BE) + DB unique index | FR-002 |
| Schema name unique per group | Sync client check (FE) + handler pre-check (BE) + DB unique index | FR-011 |
| Resource property required check | Reactive Form / signal-driven validation (FE) + handler validation (BE) | FR-020 |
| Cursor `take` bounds | Clamped in handler to `[1, 50]` regardless of query string | FR-025 |
| Members read-only for non-owner | Endpoint authorization check + UI hiding (FE) | FR-009 |
| Owner-only mutations | Authorization in handlers (BE) + UI hiding (FE) | FR-036, FR-009 |
| Authenticated user required | JWT bearer middleware (BE) + authGuard (FE) | FR-035 |
