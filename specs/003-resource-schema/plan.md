# Implementation Plan: Resource Schema Management

**Branch**: `003-resource-schema` | **Date**: 2026-05-11 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `specs/003-resource-schema/spec.md`

## Summary

Implement a generic, owner-defined resource schema system inside the existing Resources module. Group owners define ResourceTypes (schema templates with typed property definitions) and register ResourceInstances (concrete bookable items with EAV property values). Cross-module group ownership and membership authorisation is achieved via local read-models populated by four new integration events published by ManagementGroups.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: ASP.NET Core 10 Minimal API, EF Core 10 (Npgsql), Serilog, IDispatcher, IMessageBroker
**Storage**: PostgreSQL 17, schema `resources`
**Testing**: xUnit, EF Core integration tests (Testcontainers), Respawn
**Target Platform**: Linux server (Docker)
**Project Type**: Module within modular monolith web service
**Performance Goals**: Standard web API — no special requirements for this feature
**Constraints**: Module isolation (no direct cross-module references), Simplified DDD (no MediatR, no AutoMapper), GUID v7 everywhere
**Scale/Scope**: Single-tenant per group; no pagination required for this iteration

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Rule | Status | Notes |
|---|---|---|
| Module isolation — no direct cross-module references | PASS | Resources ↔ ManagementGroups via IMessageBroker events only |
| Two projects per module (Api + Core) | PASS | Scaffold already exists |
| Minimal API — no MVC controllers | PASS | All endpoints in `ResourcesModule.Expose()` |
| IDispatcher — no MediatR | PASS | Commands/queries dispatched via IDispatcher |
| No AutoMapper | PASS | Manual mapping in query handlers |
| GUID v7 (`Guid.CreateVersion7()`) | PASS | All entity `Create()` methods generate GUID v7 |
| Private setters + private constructor on domain entities | PASS | Enforced in all four new entities |
| EF per-module DbContext with schema isolation | PASS | `resources` schema, ResourcesDbContext |
| Migrations in dedicated project | PASS | `ThingsBooksy.Modules.Resources.Migrations` exists |
| Swagger/OpenAPI via `AddEndpointsApiExplorer()` | PASS | Already called in Bootstrapper |
| Test-first | PASS | Integration tests written after implementation per workflow |

## Project Structure

### Documentation (this feature)

```text
specs/003-resource-schema/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── contracts/
│   └── endpoints.md     # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit-tasks)
```

### Source Code

```text
src/Shared/ThingsBooksy.Shared.Abstractions/
└── Events/ManagementGroups/               ← NEW directory
    ├── GroupCreated.cs
    ├── GroupDeleted.cs
    ├── GroupMemberAdded.cs
    └── GroupMemberRemoved.cs

src/Modules/ManagementGroups/ThingsBooksy.Modules.ManagementGroups.Core/
└── Features/                              ← MODIFIED handlers only
    ├── CreateManagementGroup/CreateManagementGroupHandler.cs
    ├── DeleteManagementGroup/DeleteManagementGroupHandler.cs
    ├── AddGroupMember/AddGroupMemberHandler.cs
    └── RemoveGroupMember/RemoveGroupMemberHandler.cs

src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/
├── DAL/
│   ├── ResourcesDbContext.cs              ← MODIFIED (add DbSets)
│   ├── ResourcesUnitOfWork.cs             (unchanged)
│   └── Configurations/                    ← NEW
│       ├── ResourceTypeConfiguration.cs
│       ├── ResourcePropertyDefinitionConfiguration.cs
│       ├── ResourceInstanceConfiguration.cs
│       ├── ResourcePropertyValueConfiguration.cs
│       ├── GroupReadModelConfiguration.cs
│       └── GroupMemberReadModelConfiguration.cs
├── Domain/                                ← NEW
│   ├── PropertyDataType.cs
│   ├── ResourceType.cs
│   ├── ResourcePropertyDefinition.cs
│   ├── ResourceInstance.cs
│   └── ResourcePropertyValue.cs
├── ReadModels/                            ← NEW
│   ├── GroupReadModel.cs
│   └── GroupMemberReadModel.cs
├── Events/Handlers/                       ← NEW
│   ├── GroupCreatedHandler.cs
│   ├── GroupDeletedHandler.cs
│   ├── GroupMemberAddedHandler.cs
│   └── GroupMemberRemovedHandler.cs
├── Exceptions/                            ← NEW
│   ├── ResourcesDomainException.cs
│   └── ResourcesForbiddenException.cs
└── Features/                              ← NEW
    ├── CreateResourceType/
    │   ├── CreateResourceTypeCommand.cs
    │   └── CreateResourceTypeHandler.cs
    ├── UpdateResourceType/
    │   ├── UpdateResourceTypeCommand.cs
    │   └── UpdateResourceTypeHandler.cs
    ├── DeleteResourceType/
    │   ├── DeleteResourceTypeCommand.cs
    │   └── DeleteResourceTypeHandler.cs
    ├── GetResourceType/
    │   ├── GetResourceTypeQuery.cs
    │   └── GetResourceTypeHandler.cs
    ├── GetResourceTypes/
    │   ├── GetResourceTypesQuery.cs
    │   └── GetResourceTypesHandler.cs
    ├── CreateResourceInstance/
    │   ├── CreateResourceInstanceCommand.cs
    │   └── CreateResourceInstanceHandler.cs
    ├── UpdateResourceInstance/
    │   ├── UpdateResourceInstanceCommand.cs
    │   └── UpdateResourceInstanceHandler.cs
    ├── DeleteResourceInstance/
    │   ├── DeleteResourceInstanceCommand.cs
    │   └── DeleteResourceInstanceHandler.cs
    ├── GetResourceInstance/
    │   ├── GetResourceInstanceQuery.cs
    │   └── GetResourceInstanceHandler.cs
    └── GetResourceInstances/
        ├── GetResourceInstancesQuery.cs
        └── GetResourceInstancesHandler.cs

src/Modules/Resources/ThingsBooksy.Modules.Resources.Api/
└── ResourcesModule.cs                     ← MODIFIED (endpoints + event handler registrations)

src/Modules/Resources/ThingsBooksy.Modules.Resources.Migrations/
└── Migrations/                            ← NEW (generated by migration-agent)
```

**Structure Decision**: Modular monolith — all new code lives in the existing Resources and ManagementGroups modules. No new module or project is created.

## Implementation Phases

### Wave 1 — Shared Contracts (no dependencies)

**Module: Shared.Abstractions**
- Add `Events/ManagementGroups/GroupCreated.cs`
- Add `Events/ManagementGroups/GroupDeleted.cs`
- Add `Events/ManagementGroups/GroupMemberAdded.cs`
- Add `Events/ManagementGroups/GroupMemberRemoved.cs`

These are net-new `IEvent` records. Must exist before any handler can reference them.

---

### Wave 2 — ManagementGroups Event Publishing (depends on Wave 1)

**Module: ManagementGroups**

Update four existing handlers to inject `IMessageBroker` and publish events after `SaveChangesAsync`:

1. `CreateManagementGroupHandler` → `await _messageBroker.PublishAsync(new GroupCreated(group.Id, group.OwnerId))`
2. `DeleteManagementGroupHandler` → `await _messageBroker.PublishAsync(new GroupDeleted(command.GroupId))`
3. `AddGroupMemberHandler` → `await _messageBroker.PublishAsync(new GroupMemberAdded(command.GroupId, userReadModel.Id))`
4. `RemoveGroupMemberHandler` → `await _messageBroker.PublishAsync(new GroupMemberRemoved(command.GroupId, command.UserId))`

Schema changes: NONE (no new tables or columns in ManagementGroups).

---

### Wave 3 — Resources Domain & Persistence (depends on Wave 1, parallel with Wave 2)

**Module: Resources**

Implement all domain entities, read models, EF configuration, exception types, and update the DbContext:

**Domain entities** (all in `Core/Domain/`):
- `PropertyDataType` enum (Text=0, Number=1, Boolean=2) — stored as string
- `ResourceType` entity with private setters, private constructor, `Create`/`Update`/`Delete`/`IsDeleted`
- `ResourcePropertyDefinition` entity with `Create`/`Update`
- `ResourceInstance` entity with `Create`/`Update`/`Delete`/`IsDeleted`
- `ResourcePropertyValue` entity with `Create`/`Update`

**Read models** (in `Core/ReadModels/`):
- `GroupReadModel { Guid Id, Guid OwnerId }`
- `GroupMemberReadModel { Guid GroupId, Guid UserId }` with composite PK

**Exceptions** (in `Core/Exceptions/`):
- `ResourcesDomainException : CustomException` (400)
- `ResourcesForbiddenException : ForbiddenException` (403)

**DbContext** — add to `ResourcesDbContext`:
```csharp
public DbSet<ResourceType> ResourceTypes { get; set; }
public DbSet<ResourcePropertyDefinition> ResourcePropertyDefinitions { get; set; }
public DbSet<ResourceInstance> ResourceInstances { get; set; }
public DbSet<ResourcePropertyValue> ResourcePropertyValues { get; set; }
public DbSet<GroupReadModel> GroupReadModels { get; set; }
public DbSet<GroupMemberReadModel> GroupMemberReadModels { get; set; }
```

**EF Configurations** (all in `Core/DAL/Configurations/`):
- `ResourceTypeConfiguration`: table `resource_types`, global query filter `x => !x.IsDeleted`, PropertyDefinitions navigation owned-entity cascade
- `ResourcePropertyDefinitionConfiguration`: table `resource_property_definitions`, DataType stored as string
- `ResourceInstanceConfiguration`: table `resource_instances`, global query filter `x => !x.IsDeleted`, PropertyValues navigation cascade
- `ResourcePropertyValueConfiguration`: table `resource_property_values`
- `GroupReadModelConfiguration`: table `group_read_models`
- `GroupMemberReadModelConfiguration`: table `group_member_read_models`, composite PK `(GroupId, UserId)`

Schema changes: **NEW** — 6 new tables.

---

### Wave 4 — Resources Features (depends on Wave 3)

Implement command/query handlers and event handlers.

**Event handlers** (in `Core/Events/Handlers/`):
- `GroupCreatedHandler`: upsert `GroupReadModel { Id = GroupId, OwnerId }`
- `GroupDeletedHandler`: remove `GroupReadModel`; soft-delete all `ResourceType` and `ResourceInstance` rows for that group
- `GroupMemberAddedHandler`: insert `GroupMemberReadModel { GroupId, UserId }`
- `GroupMemberRemovedHandler`: delete `GroupMemberReadModel` row

**ResourceType handlers** (each in own `Features/` subfolder):
- `CreateResourceTypeHandler`: validate owner, create `ResourceType` + `ResourcePropertyDefinition` rows
- `UpdateResourceTypeHandler`: validate owner, update name/description, reconcile property definitions (add/update/remove)
- `DeleteResourceTypeHandler`: validate owner, check zero instances, hard-delete type + definitions
- `GetResourceTypeHandler`: return single type with definitions (null-safe for membership check)
- `GetResourceTypesHandler`: return list filtered by groupId, caller must be group member

**ResourceInstance handlers**:
- `CreateResourceInstanceHandler`: validate owner, validate required props + data types, create `ResourceInstance` + `ResourcePropertyValue` rows
- `UpdateResourceInstanceHandler`: validate owner, validate required props + data types, reconcile property values
- `DeleteResourceInstanceHandler`: validate owner, soft-delete
- `GetResourceInstanceHandler`: return single instance with values joined to definition names/types
- `GetResourceInstancesHandler`: return list filtered by resourceTypeId and/or groupId, optional `includeDeleted`

**Value validation helper** (inline in handler or private method):
```
Text   → always valid
Number → decimal.TryParse(value, out _)
Boolean→ bool.TryParse(value, out _)
```

---

### Wave 5 — Resources API Layer (depends on Wave 4)

Update `ResourcesModule.cs`:

**Register event handlers** (in `Register()`):
```csharp
services.AddScoped<IEventHandler<GroupCreated>, GroupCreatedHandler>();
services.AddScoped<IEventHandler<GroupDeleted>, GroupDeletedHandler>();
services.AddScoped<IEventHandler<GroupMemberAdded>, GroupMemberAddedHandler>();
services.AddScoped<IEventHandler<GroupMemberRemoved>, GroupMemberRemovedHandler>();
```

**Expose endpoints** (in `Expose()`):
- POST   /resources/types
- GET    /resources/types/{id}
- GET    /resources/types
- PUT    /resources/types/{id}
- DELETE /resources/types/{id}
- POST   /resources/instances
- GET    /resources/instances/{id}
- GET    /resources/instances
- PUT    /resources/instances/{id}
- DELETE /resources/instances/{id}

All endpoints: `.RequireAuthorization().WithTags("Resources")`

**DTOs** (records in Api project):
- `CreateResourceTypeRequest`, `UpdateResourceTypeRequest`
- `CreateResourceInstanceRequest`, `UpdateResourceInstanceRequest`
- `ResourceTypeDto`, `PropertyDefinitionDto`
- `ResourceInstanceDto`, `PropertyValueDto`
- `PropertyDefinitionInputDto` (used in both create and update)

**UserId extraction** (same pattern as ManagementGroups):
```csharp
private static Guid GetUserId(HttpContext context)
    => string.IsNullOrWhiteSpace(context.User.Identity?.Name)
        ? Guid.Empty
        : Guid.Parse(context.User.Identity.Name);
```

Schema changes for Wave 5: NONE.

---

### Wave 6 — Migration (handled by migration-agent after Wave 3–5)

Schema changes from Wave 3 are reported by `module-writer` and picked up automatically by the `migration-agent`. No manual migration commands needed here.

Expected new tables: `resource_types`, `resource_property_definitions`, `resource_instances`, `resource_property_values`, `group_read_models`, `group_member_read_models`.

---

## Key Implementation Notes

1. **IMessageBroker injection in ManagementGroups handlers**: currently none of the 4 handlers inject `IMessageBroker`. Add it as a constructor parameter. The handler remains `ICommandHandler<T>` — no interface changes.

2. **Global query filters**: Apply `modelBuilder.Entity<ResourceType>().HasQueryFilter(x => !x.IsDeleted)` and same for `ResourceInstance`. This ensures soft-deleted rows are excluded from all queries by default. Use `.IgnoreQueryFilters()` in `DeleteResourceTypeHandler` to verify zero instances including deleted ones.

3. **PropertyDefinition reconciliation on UpdateResourceType**: load existing definitions from DB. For each input item: if `id` provided and exists → update; if `id` not provided → add new with `Guid.CreateVersion7()`; existing definitions not present in input → delete (cascade will remove property values? No — check and delete property values first, then definition).

4. **PropertyValue reconciliation on UpdateResourceInstance**: delete all existing `ResourcePropertyValue` rows for the instance, then re-insert. Simpler than diff reconciliation and correct because the set of definitions may have changed.

5. **GroupDeletedHandler cascade**: use `ExecuteUpdateAsync`/`ExecuteDeleteAsync` (EF bulk operations) for efficiency when soft-deleting many instances on group deletion.

6. **InternalsVisibleTo**: verify `ThingsBooksy.Modules.Resources.Core.csproj` has `InternalsVisibleTo` for the integration test project. Add if missing.

7. **`resources` schema in Respawn**: already present in `ThingsBooksyWebAppFactory.cs` — no change needed.

8. **Exception mapper**: verify `ResourcesDomainException` and `ResourcesForbiddenException` are handled by the existing exception mapper. `ForbiddenException` base is already mapped; `CustomException` base returns 400 — both consistent with existing ManagementGroups pattern.
