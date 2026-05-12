# Tasks: Resource Schema Management

**Input**: Design documents from `specs/003-resource-schema/`
**Prerequisites**: plan.md ✅ spec.md ✅ research.md ✅ data-model.md ✅ contracts/ ✅

**Organization**: Grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US5)

---

## Phase 1: Setup — Shared Event Contracts

**Purpose**: Add the four integration event records that both ManagementGroups and Resources depend on. Nothing else can start until these exist.

- [ ] T001 Create `GroupCreated` event record in `src/Shared/ThingsBooksy.Shared.Abstractions/Events/ManagementGroups/GroupCreated.cs` — `public record GroupCreated(Guid GroupId, Guid OwnerId) : IEvent;`
- [ ] T002 [P] Create `GroupDeleted` event record in `src/Shared/ThingsBooksy.Shared.Abstractions/Events/ManagementGroups/GroupDeleted.cs` — `public record GroupDeleted(Guid GroupId) : IEvent;`
- [ ] T003 [P] Create `GroupMemberAdded` event record in `src/Shared/ThingsBooksy.Shared.Abstractions/Events/ManagementGroups/GroupMemberAdded.cs` — `public record GroupMemberAdded(Guid GroupId, Guid UserId) : IEvent;`
- [ ] T004 [P] Create `GroupMemberRemoved` event record in `src/Shared/ThingsBooksy.Shared.Abstractions/Events/ManagementGroups/GroupMemberRemoved.cs` — `public record GroupMemberRemoved(Guid GroupId, Guid UserId) : IEvent;`

**Checkpoint**: Shared.Abstractions compiles with four new event types.

---

## Phase 2: Foundational — ManagementGroups Event Publishing + Resources Domain

**Purpose**: Update ManagementGroups to publish events, build all Resources domain entities, read models, EF configuration, and event handlers. MUST be complete before any user story can be tested.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

### ManagementGroups — Publish Events (depends on Phase 1)

- [ ] T005 Inject `IMessageBroker` into `CreateManagementGroupHandler` and publish `GroupCreated(group.Id, group.OwnerId)` after `SaveChangesAsync` in `src/Modules/ManagementGroups/ThingsBooksy.Modules.ManagementGroups.Core/Features/CreateManagementGroup/CreateManagementGroupHandler.cs`
- [ ] T006 [P] Inject `IMessageBroker` into `DeleteManagementGroupHandler` and publish `GroupDeleted(command.GroupId)` after `SaveChangesAsync` in `src/Modules/ManagementGroups/ThingsBooksy.Modules.ManagementGroups.Core/Features/DeleteManagementGroup/DeleteManagementGroupHandler.cs`
- [ ] T007 [P] Inject `IMessageBroker` into `AddGroupMemberHandler` and publish `GroupMemberAdded(command.GroupId, userReadModel.Id)` after `SaveChangesAsync` in `src/Modules/ManagementGroups/ThingsBooksy.Modules.ManagementGroups.Core/Features/AddGroupMember/AddGroupMemberHandler.cs`
- [ ] T008 [P] Inject `IMessageBroker` into `RemoveGroupMemberHandler` and publish `GroupMemberRemoved(command.GroupId, command.UserId)` after `SaveChangesAsync` in `src/Modules/ManagementGroups/ThingsBooksy.Modules.ManagementGroups.Core/Features/RemoveGroupMember/RemoveGroupMemberHandler.cs`

### Resources — Domain Entities (depends on Phase 1, parallel with T005–T008)

- [ ] T009 Create `PropertyDataType` enum (`Text=0, Number=1, Boolean=2`) in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Domain/PropertyDataType.cs`
- [ ] T010 [P] Create `ResourceType` entity with private setters, private constructor, `Create(Guid id, Guid groupId, string name, string? description, DateTime now)`, `Update(string name, string? description, DateTime now)`, `Delete(DateTime now)`, `IsDeleted` property, and `ICollection<ResourcePropertyDefinition> PropertyDefinitions` navigation in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Domain/ResourceType.cs`
- [ ] T011 [P] Create `ResourcePropertyDefinition` entity with private setters, private constructor, `Create(Guid id, Guid resourceTypeId, string name, PropertyDataType dataType, bool isRequired)`, and `Update(string name, PropertyDataType dataType, bool isRequired)` in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Domain/ResourcePropertyDefinition.cs`
- [ ] T012 [P] Create `ResourceInstance` entity with private setters, private constructor, `Create(Guid id, Guid resourceTypeId, Guid groupId, string name, Guid ownerId, DateTime now)`, `Update(string name, DateTime now)`, `Delete(DateTime now)`, `IsDeleted` property, and `ICollection<ResourcePropertyValue> PropertyValues` navigation in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Domain/ResourceInstance.cs`
- [ ] T013 [P] Create `ResourcePropertyValue` entity with private setters, private constructor, `Create(Guid id, Guid resourceInstanceId, Guid propertyDefinitionId, string value)`, and `Update(string value)` in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Domain/ResourcePropertyValue.cs`

### Resources — Read Models & Exceptions

- [ ] T014 [P] Create `GroupReadModel` class (`Guid Id`, `Guid OwnerId` — public setters, no constructor, same pattern as `UserReadModel`) in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/ReadModels/GroupReadModel.cs`
- [ ] T015 [P] Create `GroupMemberReadModel` class (`Guid GroupId`, `Guid UserId` — public setters, no constructor) in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/ReadModels/GroupMemberReadModel.cs`
- [ ] T016 [P] Create `ResourcesDomainException : CustomException` (maps to 400) in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Exceptions/ResourcesDomainException.cs`
- [ ] T017 [P] Create `ResourcesForbiddenException : ForbiddenException` (maps to 403) in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Exceptions/ResourcesForbiddenException.cs`

### Resources — DbContext & EF Configurations (depends on T009–T017)

- [ ] T018 Add `DbSet<ResourceType> ResourceTypes`, `DbSet<ResourcePropertyDefinition> ResourcePropertyDefinitions`, `DbSet<ResourceInstance> ResourceInstances`, `DbSet<ResourcePropertyValue> ResourcePropertyValues`, `DbSet<GroupReadModel> GroupReadModels`, `DbSet<GroupMemberReadModel> GroupMemberReadModels` to `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/DAL/ResourcesDbContext.cs`
- [ ] T019 [P] Create `ResourceTypeConfiguration` (table `resource_types`, global query filter `x => !x.IsDeleted`, cascade delete for `PropertyDefinitions`) in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/DAL/Configurations/ResourceTypeConfiguration.cs`
- [ ] T020 [P] Create `ResourcePropertyDefinitionConfiguration` (table `resource_property_definitions`, `DataType` stored as string via `HasConversion<string>()`) in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/DAL/Configurations/ResourcePropertyDefinitionConfiguration.cs`
- [ ] T021 [P] Create `ResourceInstanceConfiguration` (table `resource_instances`, global query filter `x => !x.IsDeleted`, cascade delete for `PropertyValues`) in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/DAL/Configurations/ResourceInstanceConfiguration.cs`
- [ ] T022 [P] Create `ResourcePropertyValueConfiguration` (table `resource_property_values`) in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/DAL/Configurations/ResourcePropertyValueConfiguration.cs`
- [ ] T023 [P] Create `GroupReadModelConfiguration` (table `group_read_models`, PK on `Id`) in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/DAL/Configurations/GroupReadModelConfiguration.cs`
- [ ] T024 [P] Create `GroupMemberReadModelConfiguration` (table `group_member_read_models`, composite PK `(GroupId, UserId)`) in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/DAL/Configurations/GroupMemberReadModelConfiguration.cs`

### Resources — Integration Event Handlers (depends on T014–T018)

- [ ] T025 Create `GroupCreatedHandler : IEventHandler<GroupCreated>` — upsert `GroupReadModel { Id = GroupId, OwnerId }` in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Events/Handlers/GroupCreatedHandler.cs`
- [ ] T026 [P] Create `GroupDeletedHandler : IEventHandler<GroupDeleted>` — remove `GroupReadModel`; soft-delete (set `DeletedAt`) all `ResourceType` and `ResourceInstance` rows for that group in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Events/Handlers/GroupDeletedHandler.cs`
- [ ] T027 [P] Create `GroupMemberAddedHandler : IEventHandler<GroupMemberAdded>` — insert `GroupMemberReadModel { GroupId, UserId }` (skip if already exists) in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Events/Handlers/GroupMemberAddedHandler.cs`
- [ ] T028 [P] Create `GroupMemberRemovedHandler : IEventHandler<GroupMemberRemoved>` — delete `GroupMemberReadModel` row matching `(GroupId, UserId)` in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Events/Handlers/GroupMemberRemovedHandler.cs`
- [ ] T029 Register all four event handlers in `ResourcesModule.Register()`: `services.AddScoped<IEventHandler<GroupCreated>, GroupCreatedHandler>()` etc. in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Api/ResourcesModule.cs`

**Checkpoint**: Foundation complete. Resources module compiles; group events flow from ManagementGroups into Resources read-models. All user stories can now be implemented.

---

## Phase 3: User Story 1 — Define a Resource Type (P1) 🎯 MVP

**Goal**: Group owners can create resource types with custom property definitions.

**Independent Test**: `POST /resources/types` with a valid owner JWT, a group ID present in `GroupReadModels`, and two property definitions (one required Number, one optional Text) returns `201` with an `id`. A second call with a non-owner JWT returns `403`.

### Implementation (depends on Phase 2)

- [ ] T030 [US1] Create `CreateResourceTypeCommand(Guid TypeId, Guid GroupId, string Name, string? Description, IEnumerable<PropertyDefinitionInput> PropertyDefinitions)` and `PropertyDefinitionInput(string Name, PropertyDataType DataType, bool IsRequired)` records in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Features/CreateResourceType/CreateResourceTypeCommand.cs`
- [ ] T031 [US1] Create `CreateResourceTypeHandler` — validate caller is group owner (via `GroupReadModels`), validate name non-empty, create `ResourceType` + `ResourcePropertyDefinition` rows using `Guid.CreateVersion7()` for each, save — in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Features/CreateResourceType/CreateResourceTypeHandler.cs`
- [ ] T032 [P] [US1] Create `CreateResourceTypeRequest` and `PropertyDefinitionInputDto` request records, add `POST /resources/types` endpoint (extracts `userId` from JWT, dispatches `CreateResourceTypeCommand`, returns `201 Created`) in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Api/ResourcesModule.cs`

**Checkpoint**: US1 fully functional. Owner can define a resource type with custom fields via `POST /resources/types`.

---

## Phase 4: User Story 2 — Add Resource Instances (P2)

**Goal**: Group owners can register concrete resource instances of a given type, supplying values for all defined properties.

**Independent Test**: After creating a "Car" type with a required `Power (Number)` field, `POST /resources/instances` with the type ID, group ID, name, and `propertyValues: [{propertyDefinitionId, value: "116"}]` returns `201`. A request missing the required property returns `400`.

### Implementation (depends on Phase 3)

- [ ] T033 [US2] Create `CreateResourceInstanceCommand(Guid InstanceId, Guid ResourceTypeId, Guid GroupId, string Name, Guid OwnerId, IEnumerable<PropertyValueInput> PropertyValues)` and `PropertyValueInput(Guid PropertyDefinitionId, string Value)` records in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Features/CreateResourceInstance/CreateResourceInstanceCommand.cs`
- [ ] T034 [US2] Create `CreateResourceInstanceHandler` — validate caller is group owner, load `ResourceType` with its `PropertyDefinitions`, validate all required definitions have values and values match their `DataType` (`decimal.TryParse` for Number, `bool.TryParse` for Boolean), create `ResourceInstance` + `ResourcePropertyValue` rows — in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Features/CreateResourceInstance/CreateResourceInstanceHandler.cs`
- [ ] T035 [P] [US2] Create `CreateResourceInstanceRequest` and `PropertyValueInputDto` request records, add `POST /resources/instances` endpoint in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Api/ResourcesModule.cs`

**Checkpoint**: US2 functional. Owner can register instances with typed EAV property values via `POST /resources/instances`.

---

## Phase 5: User Story 3 — View Resource Types and Instances (P2)

**Goal**: Authenticated group members can list and retrieve resource types (with definitions) and resource instances (with values).

**Independent Test**: A group member JWT can call `GET /resources/types?groupId={id}` and receive the created type with its property definitions. `GET /resources/instances?resourceTypeId={id}` returns all non-deleted instances with their property values. A non-member JWT receives `403` on both.

### Implementation (depends on Phase 4)

- [ ] T036 [US3] Create `GetResourceTypeQuery(Guid TypeId, Guid RequesterId)` and `GetResourceTypeHandler` — check caller is group member (via `GroupMemberReadModels` or is group owner via `GroupReadModels`), load type with definitions, return `ResourceTypeDto` or null — in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Features/GetResourceType/`
- [ ] T037 [P] [US3] Create `GetResourceTypesQuery(Guid GroupId, Guid RequesterId)` and `GetResourceTypesHandler` — validate member, return list of `ResourceTypeDto` for group — in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Features/GetResourceTypes/`
- [ ] T038 [P] [US3] Create `GetResourceInstanceQuery(Guid InstanceId, Guid RequesterId)` and `GetResourceInstanceHandler` — validate member, load instance with property values joined to definition names and data types, return `ResourceInstanceDto` or null — in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Features/GetResourceInstance/`
- [ ] T039 [P] [US3] Create `GetResourceInstancesQuery(Guid? ResourceTypeId, Guid? GroupId, bool IncludeDeleted, Guid RequesterId)` and `GetResourceInstancesHandler` — validate member, filter by typeId/groupId, honour `IncludeDeleted` via `IgnoreQueryFilters()` when true, return list — in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Features/GetResourceInstances/`
- [ ] T040 [P] [US3] Create `ResourceTypeDto`, `PropertyDefinitionDto`, `ResourceInstanceDto`, `PropertyValueDto` response records in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Api/ResourcesModule.cs`
- [ ] T041 [US3] Add `GET /resources/types/{id}`, `GET /resources/types`, `GET /resources/instances/{id}`, `GET /resources/instances` endpoints in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Api/ResourcesModule.cs`

**Checkpoint**: US3 functional. Members can browse types and instances with full property data.

---

## Phase 6: User Story 4 — Update and Remove Resource Instances (P3)

**Goal**: Group owners can update instance names and property values, and soft-delete instances.

**Independent Test**: Owner calls `PUT /resources/instances/{id}` with an updated name and values — returns `204` and changes persist. `DELETE /resources/instances/{id}` returns `204`; subsequent `GET /resources/instances` no longer includes the item; `GET /resources/instances?includeDeleted=true` shows it with `deletedAt` set.

### Implementation (depends on Phase 5)

- [ ] T042 [US4] Create `UpdateResourceInstanceCommand(Guid InstanceId, string Name, IEnumerable<PropertyValueInput> PropertyValues, Guid RequesterId)` and `UpdateResourceInstanceHandler` — validate owner, validate required props + data types, delete all existing `ResourcePropertyValue` rows for the instance then re-insert — in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Features/UpdateResourceInstance/`
- [ ] T043 [P] [US4] Create `DeleteResourceInstanceCommand(Guid InstanceId, Guid RequesterId)` and `DeleteResourceInstanceHandler` — validate owner, call `instance.Delete(now)`, save — in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Features/DeleteResourceInstance/`
- [ ] T044 [US4] Create `UpdateResourceInstanceRequest` record, add `PUT /resources/instances/{id}` and `DELETE /resources/instances/{id}` endpoints in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Api/ResourcesModule.cs`

**Checkpoint**: US4 functional. Instance lifecycle management (update + soft-delete) is complete.

---

## Phase 7: User Story 5 — Update and Remove Resource Types (P3)

**Goal**: Group owners can update resource type name, description, and property definitions. They can hard-delete a type only when it has no instances.

**Independent Test**: Owner calls `PUT /resources/types/{id}` adding a new optional property definition — returns `204`; `GET /resources/types/{id}` reflects the change and existing instances remain valid. `DELETE /resources/types/{id}` on a type with instances returns `400`; on a type with zero instances returns `204`.

### Implementation (depends on Phase 5)

- [ ] T045 [US5] Create `UpdateResourceTypeCommand(Guid TypeId, string Name, string? Description, IEnumerable<PropertyDefinitionUpdateInput> PropertyDefinitions, Guid RequesterId)` and `PropertyDefinitionUpdateInput(Guid? Id, string Name, PropertyDataType DataType, bool IsRequired)` records in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Features/UpdateResourceType/UpdateResourceTypeCommand.cs`
- [ ] T046 [US5] Create `UpdateResourceTypeHandler` — validate owner, load existing definitions, reconcile: entries with `Id` → update, entries without `Id` → add new (`Guid.CreateVersion7()`), existing definitions absent from input → remove — in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Features/UpdateResourceType/UpdateResourceTypeHandler.cs`
- [ ] T047 [P] [US5] Create `DeleteResourceTypeCommand(Guid TypeId, Guid RequesterId)` and `DeleteResourceTypeHandler` — validate owner, check zero instances with `IgnoreQueryFilters()`, hard-delete type (cascade removes definitions) — in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Features/DeleteResourceType/`
- [ ] T048 [US5] Create `UpdateResourceTypeRequest` and `PropertyDefinitionUpdateInputDto` records, add `PUT /resources/types/{id}` and `DELETE /resources/types/{id}` endpoints in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Api/ResourcesModule.cs`

**Checkpoint**: All 5 user stories complete. Full CRUD on both ResourceTypes and ResourceInstances is operational.

---

## Phase 8: Polish & Cross-Cutting

- [ ] T049 [P] Verify `InternalsVisibleTo("ThingsBooksy.Modules.Resources.IntegrationTests")` is declared in `src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/ThingsBooksy.Modules.Resources.Core.csproj` — add if missing
- [ ] T050 [P] Run `dotnet build` from repo root and confirm zero errors and zero warnings
- [ ] T051 Run `dotnet format` from repo root and commit any formatting changes

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1** (T001–T004): No dependencies — start immediately
- **Phase 2** (T005–T029): Depends on Phase 1 — blocks all user stories
  - T005–T008 (ManagementGroups): parallel after Phase 1
  - T009–T017 (domain + read models): parallel after Phase 1
  - T018–T024 (DbContext + configs): depends on T009–T017
  - T025–T029 (event handlers): depends on T014–T018
- **Phase 3** (T030–T032) [US1]: Depends on Phase 2
- **Phase 4** (T033–T035) [US2]: Depends on Phase 3 (needs ResourceType to exist for validation)
- **Phase 5** (T036–T041) [US3]: Depends on Phase 4 (reads types and instances)
- **Phase 6** (T042–T044) [US4]: Depends on Phase 5 (update/delete existing instances)
- **Phase 7** (T045–T048) [US5]: Depends on Phase 5 (update/delete existing types)
- **Phase 8** (T049–T051): Depends on all prior phases

### Module Assignment

| Tasks | Module |
|---|---|
| T001–T004 | `Shared.Abstractions` |
| T005–T008 | `ManagementGroups` |
| T009–T051 (except T005–T008) | `Resources` |

### Parallel Opportunities Within Phase 2

```
After Phase 1 completes, launch in parallel:
  → T005, T006, T007, T008     (ManagementGroups handlers — different files)
  → T009, T010, T011, T012,
    T013, T014, T015, T016, T017  (Resources domain — all different files)

After T009–T017 complete, launch in parallel:
  → T018 (DbContext)
  → T019, T020, T021, T022, T023, T024  (EF configs — different files)

After T018–T024 complete:
  → T025, T026, T027, T028  (event handlers — different files)
  → T029 (event handler registration in module)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Shared event contracts
2. Complete Phase 2: Foundation
3. Complete Phase 3: US1 — Create ResourceType
4. **VALIDATE**: Owner can define a resource type with custom fields. Non-owner is rejected.

### Incremental Delivery

1. Phase 1 + 2 → Foundation
2. + Phase 3 → Type creation (US1 — MVP)
3. + Phase 4 → Instance creation (US2)
4. + Phase 5 → Read access for members (US3)
5. + Phase 6 → Instance lifecycle (US4)
6. + Phase 7 → Type update/delete (US5)

---

## Notes

- All entity IDs use `Guid.CreateVersion7()` inside `Create(...)` — never `Guid.NewGuid()`
- `GroupMemberReadModel` check: the group owner is NOT automatically in `GroupMemberReadModels` — ownership is stored separately in `GroupReadModels`. Handlers must check EITHER `GroupReadModels.OwnerId == callerId` OR `GroupMemberReadModels` contains `(GroupId, callerId)` for read access.
- `IgnoreQueryFilters()` required in `DeleteResourceTypeHandler` to count soft-deleted instances, and in `GetResourceInstancesHandler` when `IncludeDeleted=true`.
- Property value reconciliation in `UpdateResourceInstanceHandler`: delete-all + re-insert is simpler and correct because the set of definitions may have changed since the last update.
- `PropertyDataType` enum must use `HasConversion<string>()` in EF configuration to survive enum renaming.
