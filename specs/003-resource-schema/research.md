# Research: Resource Schema Management

**Branch**: `003-resource-schema` | **Phase**: 0

## Decision 1: Dynamic Property Storage — EAV vs JSON Column

**Decision**: EAV (Entity-Attribute-Value) using two fixed tables: `ResourcePropertyDefinition` and `ResourcePropertyValue`.

**Rationale**: A JSON column on PostgreSQL (`jsonb`) is opaque to EF Core's LINQ query provider. Microsoft.AspNetCore.OData cannot generate `$filter` expressions against jsonb field paths because EF Core has no compile-time knowledge of the keys. EAV stores each value as a real row, making it fully queryable with standard LINQ/SQL joins. This keeps the door open for OData filtering on custom property values in a future session without any schema change.

**Alternatives considered**:
- `jsonb` column: simpler writes, but OData filtering on custom fields requires raw SQL interceptors — ruled out
- Typed EAV (separate `string_value`, `int_value`, `bool_value` columns): avoids cast-on-read but adds complexity with no immediate benefit — ruled out

---

## Decision 2: Inter-Module Communication — Event-Driven Read-Models

**Decision**: ManagementGroups publishes four new integration events; Resources subscribes and maintains local read-models (`GroupReadModel`, `GroupMemberReadModel`).

**Rationale**: IModuleClient query-at-request-time would couple Resources to ManagementGroups at runtime. Event-driven read-models keep the modules fully decoupled — Resources authorises requests using only its own database, never querying ManagementGroups at query time.

**New events** (to be added in `Shared.Abstractions/Events/ManagementGroups/`):
- `GroupCreated { GroupId, OwnerId }`
- `GroupDeleted { GroupId }`
- `GroupMemberAdded { GroupId, UserId }`
- `GroupMemberRemoved { GroupId, UserId }`

**ManagementGroups handlers that must be updated**:
- `CreateManagementGroupHandler` → publish `GroupCreated` after `SaveChangesAsync`
- `DeleteManagementGroupHandler` → publish `GroupDeleted` after `SaveChangesAsync`
- `AddGroupMemberHandler` → publish `GroupMemberAdded` after `SaveChangesAsync`
- `RemoveGroupMemberHandler` → publish `GroupMemberRemoved` after `SaveChangesAsync`

**Alternatives considered**:
- IModuleClient synchronous query: simpler, but adds runtime cross-module coupling — ruled out
- Trusting JWT claims (no cross-module check): insecure, group may not exist — ruled out

---

## Decision 3: PropertyDataType Enum

**Decision**: `PropertyDataType` enum with three values: `Text`, `Number`, `Boolean`.

**Rationale**: Matches the spec exactly. Stored as a string in EF to survive enum renaming. Value validation on write:
- `Text`: always valid
- `Number`: `decimal.TryParse` (covers int and float)
- `Boolean`: `bool.TryParse`

Values are stored as `string` in `ResourcePropertyValue.Value` and cast on read.

---

## Decision 4: ResourceType Hard-Delete Constraint

**Decision**: ResourceType hard-delete is only allowed when the type has no associated instances (including soft-deleted ones). If instances exist, return 400 Bad Request.

**Rationale**: Deleting a type with instances would leave orphaned `ResourceInstance` and `ResourcePropertyValue` rows with no parent schema to interpret them. Requiring zero instances keeps referential integrity without needing a cascade.

---

## Decision 5: GroupDeleted Cascade Behaviour

**Decision**: When a `GroupDeleted` event is received, the Resources module removes the `GroupReadModel` row and soft-deletes all `ResourceInstances` belonging to that group. `ResourceTypes` for that group are also soft-deleted (add `DeletedAt` to `ResourceType`).

**Rationale**: Preserves data for audit purposes while ensuring the group's resources no longer appear in active listings. Aligns with the soft-delete pattern used for groups and instances elsewhere in the codebase.

---

## Decision 6: WebAppFactory — resources schema already included

**Observation**: `ThingsBooksyWebAppFactory.cs` already has `"resources"` in `SchemasToInclude`. No manual step is required before integration tests run.

---

## Decision 7: InternalsVisibleTo for Resources module

**Observation**: Integration tests for Resources already exist as a scaffolded project. The `Resources.Core` assembly must have `[assembly: InternalsVisibleTo("ThingsBooksy.Modules.Resources.IntegrationTests")]` added to allow tests to access internal types. Check if this already exists via AssemblyInfo or csproj before adding.
