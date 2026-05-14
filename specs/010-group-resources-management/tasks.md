---

description: "Implementation tasks for feature 010 — Group Detail, Schema Designer, Group/Resource Modals, Notifications"
---

# Tasks: Group Detail & Schema Designer & Group/Resource Modals

**Input**: Design documents from `/specs/010-group-resources-management/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md, ADR-001, ADR-002
**Tests**: Backend integration tests are mandatory per Constitution V. Test-First (gate already validated in plan.md). Tasks list them explicitly; they are produced by `integration-test-writer` agent after `quality-reviewer`. FE unit tests are out of scope this iteration except where called out in Polish phase.
**Organization**: Tasks are grouped by user story to enable independent implementation and testing. Phase 2 Foundational deliberately absorbs all of US8 (Notifications) because every other story consumes toasts — calling out US8 explicitly so its Independent Test still applies once Foundational is done.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: US1–US8 mapped to spec.md user stories. Setup, Foundational, and Polish tasks have no story label.
- Exact file paths included in every task.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Lightweight prep — everything heavy is already scaffolded (BE modules exist, FE bootstrapped, tokens defined).

- [ ] T001 Create `frontend/src/styles/_animations.scss` containing keyframes `fadeUp`, `fadeDown`, `modalEnter` (translateY+scale+opacity), `draftPulse` (opacity 1→0.4→1, 2s ease-in-out infinite), and a `.no-anim *` reduced-motion override that flattens animation-duration / transition-duration to 0.001ms. Import the file from `frontend/src/styles.scss` (or wherever the global stylesheet aggregates partials).

- [ ] T002 Add `@angular/cdk@^21` to `frontend/package.json` dependencies; run `cd frontend && npm install` to lock-file it. No source code consumes it yet — Phase 4 (Schema Designer drag/drop) will. **Note**: Verify the resolved version matches the existing `@angular/*` major to avoid peer-dep warnings.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: BE schema/index migrations + FE shared primitives + Notifications. Every user story below assumes these are done.

### Backend foundational

- [ ] T003 Configure unique partial index on `ManagementGroup` in `backend/src/Modules/ManagementGroups/ThingsBooksy.Modules.ManagementGroups.Core/DAL/Configurations/ManagementGroupConfiguration.cs`: `builder.HasIndex(g => new { g.OwnerId, g.Name }).IsUnique().HasFilter("\"DeletedAt\" IS NULL")`.

- [ ] T004 [P] Configure unique index on `ResourceType` in `backend/src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/DAL/Configurations/ResourceTypeConfiguration.cs`: `builder.HasIndex(t => new { t.GroupId, t.Name }).IsUnique()`.

- [ ] T005 [P] Configure cursor index on `ResourceInstance` in `backend/src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/DAL/Configurations/ResourceInstanceConfiguration.cs`: `builder.HasIndex(i => new { i.GroupId, i.Id }).HasFilter("\"DeletedAt\" IS NULL")`.

- [ ] T006 Generate EF migration `AddGroupOwnerNameUniqueIndex` for `ManagementGroups.Migrations` (after T003). Command: `dotnet ef migrations add AddGroupOwnerNameUniqueIndex --project backend/src/Modules/ManagementGroups/ThingsBooksy.Modules.ManagementGroups.Migrations --startup-project backend/src/Bootstrapper/ThingsBooksy.Bootstrapper`. Delegated to `migration-agent` after `module-writer` reports schema changes.

- [ ] T007 [P] Generate EF migration `AddResourceTypeUniqueAndCursorIndexes` for `Resources.Migrations` (after T004 + T005). Same command, swap project paths to Resources. Delegated to `migration-agent`.

### Frontend foundational — shared primitives

- [ ] T008 [P] Implement `NotificationService` in `frontend/src/app/shared/services/notification.service.ts`: signal-backed queue `signal<Toast[]>`, methods `success(message, opts?)`, `error(message, opts?)`, `info(message, opts?)`, `dismiss(id)`; max 3 toasts stacked FIFO; per-toast `setTimeout(5000)` dismissal scheduled with `effect()` cleanup. `Toast = { id: string; kind: 'success'|'error'|'info'; message: string; createdAt: number }`. Inject-able as `providedIn: 'root'`.

- [ ] T009 [P] Implement `tb-toast` standalone component in `frontend/src/app/shared/components/toast/toast.component.ts|html|scss`. Reads `NotificationService.toasts()` signal and renders the list as glass-card pills bottom-right with `fadeUp` enter (per `_animations.scss`). Per-toast close button calls `notificationService.dismiss(id)`. ChangeDetection OnPush.

- [ ] T010 [US8] [FR-029] Mount `<tb-toast />` AND `<tb-confirm-dialog />` (the singleton host for `ConfirmDialogService`) once in `frontend/src/app/app.component.html` so they are available across all routes. The toast host renders from `NotificationService.toasts()`; the confirm-dialog host is bound to `ConfirmDialogService.state()` signal and emits `confirm`/`cancel` back to the service which resolves the Promise returned by `confirm(...)`.

- [ ] T011 [US8] Extend `frontend/src/app/core/interceptors/error.interceptor.ts` to inject `NotificationService` and call `.error(message)` on any 4xx/5xx response. Message resolution: `response.error?.message ?? response.statusText ?? 'Something went wrong'`. Skip toasting when request header `x-silent-errors: true` is present (used by name-availability check). Preserve existing error pass-through behavior.

- [ ] T012 [P] Implement `tb-confirm-dialog` standalone component in `frontend/src/app/shared/components/confirm-dialog/confirm-dialog.component.ts|html|scss` wrapping the existing `tb-modal`. Inputs: `open`, `title`, `message`, `confirmLabel='Confirm'`, `cancelLabel='Cancel'`, `danger=false`. Outputs: `confirm`, `cancel`. Plus `ConfirmDialogService` (`shared/services/confirm-dialog.service.ts`) exposing `confirm({title, message, danger}): Promise<boolean>` that mounts the dialog imperatively into a CDK overlay-less Portal-style host (or via a hidden `<tb-confirm-dialog>` mounted in `app.component.html` and bridged via signals).

- [ ] T013 [P] Implement `tb-avatar` standalone component in `frontend/src/app/shared/components/avatar/avatar.component.ts|html|scss`. Inputs: `id: string` (required), `initials: string` (required), `size: 'sm'|'md'|'lg' = 'md'`. Computes accent color in a `computed()` via FNV-1a 32-bit hash on `id`, modulo 3 → `--color-accent-primary`/`-secondary`/`-tertiary`. Optional input `colorOverride?: 'primary'|'secondary'|'tertiary'` for forward-compat.

- [ ] T014 [P] Implement `tb-type-pill` standalone component in `frontend/src/app/shared/components/type-pill/type-pill.component.ts|html|scss`. Inputs: `value: 'text'|'number'|'boolean'`, `readonly=false`. Output: `valueChange`. Click cycles or opens a small picker; visual = colored chip per `_tokens.scss` (number → primary, text → secondary, boolean → tertiary).

- [ ] T015 [P] Implement `tbInfiniteScroll` structural directive in `frontend/src/app/shared/directives/infinite-scroll.directive.ts`. Inputs: `disabled: boolean = false`, `rootMargin: string = '120px'`. Output: `loadMore`. Creates a sentinel `<div>` after the host using a `ViewContainerRef`, observes with `IntersectionObserver` (`root: null, rootMargin, threshold: 0`); emits `loadMore` when intersecting and not disabled. Disconnects observer on `ngOnDestroy`.

- [ ] T016 Register a single new lazy route in `frontend/src/app/app.routes.ts`: `{ path: 'groups', canActivate: [authGuard], loadChildren: () => import('./features/groups/groups.routes').then(m => m.groupsRoutes) }`. Create stub `frontend/src/app/features/groups/groups.routes.ts` exporting `groupsRoutes: Routes = []`. **The schemas routes are NESTED as a child of `:groupId`**, not registered as a separate top-level route, to avoid the `groups` / `groups/:groupId/schemas` sibling routing conflict. The schemas feature routes (`features/schemas/schemas.routes.ts`) are still created as a stub here (exporting `schemasRoutes: Routes = []`) and wired in T026 (under `:groupId`'s children → `{ path: 'schemas', loadChildren: () => …schemasRoutes }`).

**Checkpoint**: Foundation ready. Story phases below can start.

---

## Phase 3: User Story 1 — Create a group and land on its detail page (Priority: P1) 🎯 MVP

**Goal**: Click "Create new group" → fill modal → submit → land on `/groups/{id}` showing four panels populated with the new (mostly empty) group.

**Independent Test**: Per spec US1 Independent Test — sign in, create unique-named group, assert modal closes, toast appears, URL is `/groups/{id}`, all four panels render.

### Backend — US1

- [ ] T017 [US1] Add `IsGroupNameAvailableQuery(Guid CallerUserId, string Name)` + `IsGroupNameAvailableQueryHandler` + `IsGroupNameAvailableResult(bool Available)` + `IIsGroupNameAvailableDataProvider` in `backend/src/Modules/ManagementGroups/ThingsBooksy.Modules.ManagementGroups.Core/Features/IsGroupNameAvailable/`. Data provider method `ExistsAsync(Guid ownerId, string normalizedName, CancellationToken)` checks `_db.ManagementGroups.AnyAsync(g => g.OwnerId == ownerId && EF.Functions.ILike(g.Name, normalizedName) && g.DeletedAt == null)`. Handler normalizes name (`Trim().ToLowerInvariant()`) and returns `new IsGroupNameAvailableResult(!exists)`.

- [ ] T018 [US1] Add endpoint `GET /management-groups/name-available?name={name}` in `backend/src/Modules/ManagementGroups/ThingsBooksy.Modules.ManagementGroups.Api/ManagementGroupsModule.cs::Expose()`. Construct `IsGroupNameAvailableQuery(callerId, name)` in the lambda; reject `name` if empty / >100 chars with 400. Use `[FromQuery]` binding for `name`. Authorize via JWT (existing `RequireAuthorization()` chain).

- [ ] T019 [US1] [FR-001, FR-002] Add uniqueness pre-check to `CreateManagementGroupHandler` in `backend/src/Modules/ManagementGroups/ThingsBooksy.Modules.ManagementGroups.Core/Features/CreateManagementGroup/`. Inject a small helper data-provider method (or reuse `IIsGroupNameAvailableDataProvider` if convenient) to check before saving. On collision, throw a new domain exception `GroupNameAlreadyTakenException(string name) : ThingsBooksyException` mapped to HTTP 409 with body `{ code: "GROUP_NAME_TAKEN", message: "You already own a group with this name." }`. Map the exception in the global exception handler (locate or add mapping in `backend/src/Shared/ThingsBooksy.Shared.Infrastructure/Exceptions/` or similar central place).

- [ ] T019b [US1] [FR-007] Add `int MemberCount` field to `GetManagementGroupResult` in `backend/src/Modules/ManagementGroups/ThingsBooksy.Modules.ManagementGroups.Core/Features/GetManagementGroup/GetManagementGroupResult.cs` and populate it in `GetManagementGroupQueryHandler`: count rows in `GroupMembers` for the group + 1 (owner). The Group Header chip on FE reads this field (T024). Without this task the chip would always render 0.

### Frontend — US1

- [ ] T020 [US1] [FR-001, FR-002, FR-003, FR-005, FR-033] Replace placeholder in `frontend/src/app/features/dashboard/create-group-modal/create-group-modal.component.ts|html|scss` with `CreateOrEditGroupModalComponent`. Inputs: `mode: 'create'|'edit' = 'create'`, `initialValue?: { id: string; name: string; description: string|null }`, `open: boolean`. Outputs: `close`, `submitted`. Reactive Form with typed `FormControl<string>` for `name` (`nonNullable: true`, validators required + minLength(1) + maxLength(100)) and `FormControl<string|null>` for `description` (validator maxLength(500)). Async validator on `name` debounced via `timer(300) + first()` calling `ManagementGroupsClient.nameAvailable(name)` with header `x-silent-errors: true`. In edit mode, skip the async check when the trimmed name equals `initialValue.name`. Submit button disabled when `form.invalid || form.pending || submitting`. Submit calls `ManagementGroupsClient.create({name, description})` (create mode) or `.update(initialValue.id, {name, description})` (edit mode). On 201/200: emit `submitted` with the response, parent closes modal + navigates / refreshes.

- [ ] T021 [US1] Wire `CreateOrEditGroupModalComponent` into `frontend/src/app/features/dashboard/dashboard-page/dashboard-page.component.ts|html`: bind to existing `modalOpen` signal, set `mode="create"`, on `submitted` event close modal, call `NotificationService.success('Group created')`, and `router.navigate(['/groups', response.id])`.

- [ ] T022 [P] [US1] Create `GroupContextStore` signal store in `frontend/src/app/features/groups/group-context.store.ts`: `providedIn` scoped via the group route's component providers (not root). Holds: `groupSignal: WritableSignal<GroupDetail|null>`, `schemasSignal: WritableSignal<SchemaSummary[]>`, `membersSignal: WritableSignal<{items: MemberDto[], nextCursor: string|null}>`, `resourcesSignal: WritableSignal<{items: ResourceInstance[], nextCursor: string|null}>`, plus `loadGroup(id)`, `appendMembers(...)`, `appendResources(...)`, `replaceSchemas(...)`, mutating methods used by panels. Used by all panels — instantiated once in `group-detail-page`.

- [ ] T023 [P] [US1] Create `GroupDetailPageComponent` in `frontend/src/app/features/groups/group-detail-page/group-detail-page.component.ts|html|scss`. Resolves `:groupId` from `ActivatedRoute`, provides `GroupContextStore` in component-level `providers: [GroupContextStore]`, on init calls `store.loadGroup(id)` → fetches group, schemas, first page of resources/members in parallel. Renders the four panel components stacked with `fadeUp` staggered animations (delays 0.15s / 0.22s / 0.30s / 0.38s) inline `style` bindings. Breadcrumb `Dashboard / {Group name}` at the top, clickable back to `/dashboard`.

- [ ] T024 [US1] Create `GroupHeaderPanelComponent` in `frontend/src/app/features/groups/group-header-panel/...`. Reads group from `GroupContextStore`. Renders `tb-avatar` (id+initials), name (Outfit 800 28px gradient), description (line-clamp 2), three meta chips: created date, member count (`group.memberCount`), resource count (`store.resourcesSignal().items.length` — TODO: until total endpoint exists, use loaded count). Owner-only buttons Edit and Delete; visibility computed `viewer.id === group.ownerId`. Click Edit → emits `edit` to parent (handled in US5). Click Delete → emits `delete` (handled in US6).

- [ ] T025 [US1] Add `schemas-panel`, `resources-panel`, `members-panel` as **stub** components in `frontend/src/app/features/groups/{schemas,resources,members}-panel/` that render empty state placeholders ("No schemas yet" / "No resources yet" / member list with owner only). Full functionality lands in US2/US3/US4. The stubs read from `GroupContextStore` so subsequent stories just wire content.

- [ ] T026 [US1] Register the group detail route + nested schemas in `frontend/src/app/features/groups/groups.routes.ts`:
  ```ts
  export const groupsRoutes: Routes = [
    {
      path: ':groupId',
      loadComponent: () => import('./group-detail-page/group-detail-page.component').then(m => m.GroupDetailPageComponent),
      children: [
        { path: 'schemas', loadChildren: () => import('../schemas/schemas.routes').then(m => m.schemasRoutes) }
      ]
    }
  ];
  ```
  This nests schemas under `groups/:groupId/schemas/...` cleanly via Angular's child route mechanism. Note: this means the Schema Designer route is a sibling-rendered child (no `<router-outlet>` inside `GroupDetailPageComponent` is needed if you use `loadChildren` — the schemas routes render through the root outlet thanks to Angular 21 standalone routing). Verify with `fe-route-writer` agent that the parameter `:groupId` is accessible inside `SchemaDesignerPageComponent` via `route.paramMap` or `route.parent.paramMap`.

- [ ] T027 [US1] Update `frontend/src/app/features/dashboard/dashboard-admin-panel/dashboard-admin-panel.component.ts|html`: rows become clickable with `routerLink="/groups/{{group.id}}"`, swap inline avatar render with `<tb-avatar :id="group.id" :initials="..."/>`. Refresh group list on `submitted` from create modal.

### Tests for User Story 1

- [ ] T028 [P] [US1] Integration test `IsGroupNameAvailable_AvailableName_ReturnsTrue` in `backend/src/Modules/ManagementGroups/ThingsBooksy.Modules.ManagementGroups.IntegrationTests/.../IsGroupNameAvailableEndpointTests.cs`. Use existing TestClient + GroupFactory. Per `integration-test-naming.md`.

- [ ] T029 [P] [US1] Integration test `IsGroupNameAvailable_TakenName_ReturnsFalse` (same file). Seed a group owned by the caller, hit endpoint with same name (different casing) → expect `available: false`.

- [ ] T030 [P] [US1] Integration test `IsGroupNameAvailable_NameOfSoftDeletedGroup_ReturnsTrue` (same file). Seed + soft-delete group, hit endpoint → expect `available: true`.

- [ ] T031 [P] [US1] Integration test `IsGroupNameAvailable_DifferentOwner_NameTaken_ReturnsTrue` (same file). Confirms scoping.

- [ ] T032 [P] [US1] Integration test `CreateManagementGroup_DuplicateNameSameOwner_Returns409` in `.../CreateManagementGroupEndpointTests.cs`.

- [ ] T033 [P] [US1] Integration test `CreateManagementGroup_DuplicateNameDifferentOwner_Succeeds` (same file).

- [ ] T034 [P] [US1] Integration test `CreateManagementGroup_NameReusedAfterSoftDelete_Succeeds` (same file).

**Checkpoint**: User Story 1 is fully shippable as MVP — user can create groups with collision protection and lands on a populated (mostly empty) detail page.

---

## Phase 4: User Story 2 — Design a resource schema for a group (Priority: P1)

**Goal**: From group detail page click *Add schema* → Schema Designer page → enter name + add/reorder/edit fields → live preview reacts → save → return to group detail with new schema listed.

**Independent Test**: Per spec US2 Independent Test.

### Backend — US2

- [ ] T035 [US2] Add uniqueness pre-check to `CreateResourceTypeHandler` in `backend/src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Features/CreateResourceType/`. On collision throw `ResourceTypeNameAlreadyExistsException(Guid groupId, string name)` mapped to HTTP 409 body `{ code: "RESOURCE_TYPE_NAME_TAKEN", message: "A schema with this name already exists in the group." }`. Use a dedicated data provider method `ExistsByGroupAndNameAsync(Guid groupId, string normalizedName, Guid? excludeId, CancellationToken)`.

- [ ] T036 [US2] Add uniqueness pre-check to `UpdateResourceTypeHandler` in `.../Features/UpdateResourceType/` — same as T035, but pass `excludeId = command.Id`.

### Frontend — US2

- [ ] T037 [P] [US2] [FR-011, FR-012, FR-013, FR-014, FR-017] Create `SchemaDesignerPageComponent` in `frontend/src/app/features/schemas/schema-designer-page/...`. Reads `:groupId` and optional `:schemaId` from route. Holds local signals: `nameSignal`, `descriptionSignal`, `fieldsSignal: WritableSignal<FieldDraft[]>`, derived `dirty = computed(() => !equalsInitial(...))`. Layout: two-column grid (single column below 900px). Save/Cancel pinned bottom-right (sticky footer). Draft badge top-right ("No changes" / "Unsaved changes" with pulsing dot via `_animations.scss::draftPulse` / "Saved ✓" mint). On init: in edit mode, fetch the schema via API and seed signals; in create mode, start empty. Save call: `ResourcesClient.createResourceType({groupId, name, description, propertyDefinitions: fieldsSignal()})` or `.updateResourceType(schemaId, {name, description, propertyDefinitions})`. On success: toast "Schema saved", navigate back to `/groups/{groupId}`.

- [ ] T038 [P] [US2] [FR-015, FR-016] Create `SchemaFormPanelComponent` in `frontend/src/app/features/schemas/schema-form-panel/...`. Inputs: bound signals (name, description, fields). Renders name input, description textarea, `<div cdkDropList (cdkDropListDropped)="reorder($event)">` containing one `field-row` per field, plus `<button>Add field</button>` appending a new `{ id: crypto.randomUUID(), name: '', dataType: 'number', isRequired: false }` to the signal.

- [ ] T039 [P] [US2] Create `FieldRowComponent` in `frontend/src/app/features/schemas/field-row/...`. Inputs: `field`, `index`. Outputs: `change`, `delete`. Layout: drag handle (CDK `cdkDragHandle`), name input, `tb-type-pill` for dataType, required checkbox, trash icon. All edits emit `change` with patched field.

- [ ] T040 [P] [US2] Create `SchemaPreviewPanelComponent` in `frontend/src/app/features/schemas/schema-preview-panel/...`. Inputs: `name`, `description`, `fields`. Pure render of how the form will look to a user filling a resource instance: label + appropriate input per dataType (text → `<input type="text">`, number → `<input type="number">`, boolean → toggle/radio "Yes / No"). Required marker `*`. Sticky on desktop (`position: sticky; top: 88px`).

- [ ] T041 [US2] [FR-018] Create `UnsavedChangesGuard` (`CanDeactivateFn`) in `frontend/src/app/features/schemas/guards/unsaved-changes.guard.ts`. Imports `ConfirmDialogService`. Returns `true` if `component.dirty()` is false; otherwise opens confirm dialog "You have unsaved changes — discard them?" and returns its outcome. Also register `window.beforeunload` while dirty in `SchemaDesignerPageComponent.ngOnInit`, remove in `ngOnDestroy`.

- [ ] T042 [US2] Wire schema routes in `frontend/src/app/features/schemas/schemas.routes.ts`: `[{ path: 'new', loadComponent: () => …SchemaDesignerPageComponent, canDeactivate: [UnsavedChangesGuard] }, { path: ':schemaId', loadComponent: ..., canDeactivate: [UnsavedChangesGuard] }]`. Parent route is `groups/:groupId/schemas`, nested via T026 (not as top-level route).

- [ ] T043 [US2] Update `SchemasPanelComponent` (stub from T025) to render the real schemas list (from `GroupContextStore.schemasSignal()`) with per-row click → `router.navigate(['/groups', groupId, 'schemas', schema.id])`. Add owner-only "+ Add schema" button → `router.navigate(['/groups', groupId, 'schemas', 'new'])`.

### Tests for User Story 2

- [ ] T044 [P] [US2] Integration test `CreateResourceType_DuplicateNameInGroup_Returns409` in `backend/src/Modules/Resources/ThingsBooksy.Modules.Resources.IntegrationTests/.../CreateResourceTypeEndpointTests.cs`.

- [ ] T045 [P] [US2] Integration test `CreateResourceType_SameNameDifferentGroup_Succeeds` (same file).

- [ ] T046 [P] [US2] Integration test `UpdateResourceType_DuplicateAmongOthers_Returns409` in `.../UpdateResourceTypeEndpointTests.cs`.

- [ ] T047 [P] [US2] Integration test `UpdateResourceType_SameNameAsItself_Succeeds` (same file) — confirms `excludeId` works.

**Checkpoint**: User Story 2 shippable — owner can create and edit schemas with collision protection, drag-reorder, live preview, dirty-state guard.

---

## Phase 5: User Story 3 — Create a resource instance (Priority: P1)

**Goal**: From group detail page open Create Resource modal (global or per-schema), pick or have a schema preselected, fill dynamic fields, submit, resource appears.

**Independent Test**: Per spec US3 Independent Test.

**Backend**: no changes — existing `POST /resources/instances` is already in place and accepts dynamic property values.

### Frontend — US3

- [ ] T048 [P] [US3] [FR-020, FR-021] Create `CreateResourceModalComponent` in `frontend/src/app/features/groups/create-resource-modal/...`. Wraps `tb-modal`. Inputs: `open`, `preselectedSchemaId?: string`, `groupId`. Reads schemas from `GroupContextStore.schemasSignal()`. Renders: if `preselectedSchemaId` set → readonly chip showing schema name; else `<select>` listing schemas with empty default. Below: name (required), description (optional), and a dynamically rendered `<form>` block whose controls are built from the selected schema's `PropertyDefinitions`. Each property → one Reactive Form control with validators (required if `IsRequired`, plus type-appropriate validator: pattern for Number, true/false for Boolean). On submit: build the `propertyValues: { propertyDefinitionId, value }[]` array (every value serialized to string per ADR-001 EAV rule) and call `ResourcesClient.createResourceInstance({resourceTypeId, name, description, propertyValues})`. On success: toast "Resource created", emit `created` event with the response, parent closes modal and prepends the new instance to `GroupContextStore.resourcesSignal().items`.

- [ ] T049 [US3] [FR-022, FR-023] Update `ResourcesPanelComponent` (stub from T025) to render the resource list as a table with columns Name / Type (schema name lookup via `GroupContextStore.schemasSignal()`) / Status. Status column uses the **existing** `tb-status-badge` component at `frontend/src/app/shared/components/status-badge/`. Extend that component's `status` input to accept a new `'available'` variant (currently only `'confirmed'|'cancelled'`) — minimal change: add `case 'available'` returning success styling, and pass label `'Available (mocked)'`. If `fe-component-writer` finds the existing component too tightly typed to extend safely, fall back to a small new wrapper that maps the label to the existing success variant. Owner-only header button "+ Add resource" → opens `CreateResourceModalComponent` (without preselection). Per-schema "+" trigger in `SchemasPanelComponent` (US2) → opens same modal with `preselectedSchemaId`. Use an event from `SchemasPanelComponent` bubbling up to `GroupDetailPageComponent` which controls modal state.

### Tests for User Story 3

(No new BE tests — existing `CreateResourceInstance` flow unchanged; spec acceptance is via FE/E2E quickstart.)

- [ ] T050 [P] [US3] Smoke integration test ensuring resource instance creation still passes through after the uniqueness changes — `CreateResourceInstance_WithValidValues_Returns201` in `.../CreateResourceInstanceEndpointTests.cs` (existing test file; add a regression case if absent).

**Checkpoint**: User Story 3 shippable — owner can create resource instances from either entry point.

---

## Phase 6: User Story 4 — Browse with infinite scroll (Priority: P2)

**Goal**: Cursor-based pagination on Resources and Members lists with sentinel-driven IntersectionObserver fetch.

**Independent Test**: Per spec US4 Independent Test.

### Backend — US4

- [ ] T051 [US4] [FR-025, FR-026, FR-028] Update `GetResourceInstancesQuery` in `backend/src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Features/GetResourceInstances/GetResourceInstancesQuery.cs` to add `Guid? AfterId` and `int Take` properties; data provider implementation: `WHERE GroupId = @g AND DeletedAt IS NULL AND (@after IS NULL OR Id > @after) ORDER BY Id LIMIT @take`. Handler clamps `Take` to `[1, 50]` (default 20).

- [ ] T052 [US4] Change `GetResourceInstancesResult` to the new envelope `{ Items, NextCursor }` (see data-model.md). `NextCursor = Items.Count == Take ? Items[^1].Id : null`.

- [ ] T053 [US4] Update endpoint in `ResourcesModule.cs::Expose()` to bind `[FromQuery] Guid? afterId` and `[FromQuery] int? take`, then construct the query in the lambda. Update Swagger metadata.

- [ ] T054 [P] [US4] [FR-025, FR-026, FR-037] Add `GetGroupMembersQuery(Guid CallerUserId, Guid GroupId, Guid? AfterId, int Take)` + handler + result in `backend/src/Modules/ManagementGroups/ThingsBooksy.Modules.ManagementGroups.Core/Features/GetGroupMembers/`. Handler authorizes (caller is owner OR existing member), returns owner synthesized as `GroupMemberDto(OwnerId, OwnerEmail, group.CreatedAt, IsOwner=true)` prepended to actual `GroupMember` rows joined with `UserReadModel`. Sort by `Id` ascending. Cursor logic as in T052. Data provider exposes a method joining `GroupMember` with `UserReadModel` and including the synthetic owner row via UNION (or compose in memory after limited fetch — implementation detail for `module-writer`).

- [ ] T055 [US4] Add endpoint `GET /management-groups/{id}/members` in `ManagementGroupsModule.cs::Expose()`. Construct `GetGroupMembersQuery` from path + query params. Return `200` + body or `403`/`404` per contracts.

### Frontend — US4

- [ ] T056 [P] [US4] Wire infinite scroll in `ResourcesPanelComponent`: place `<div tbInfiniteScroll [disabled]="loading() || !nextCursor()" (loadMore)="loadMore()" />` after the table. `loadMore` calls `ResourcesClient.getResourceInstances({groupId, afterId: nextCursor(), take: 20})` and appends `items` to `GroupContextStore.resourcesSignal()`, updates `nextCursor`.

- [ ] T057 [P] [US4] Wire infinite scroll in `MembersPanelComponent` (stub from T025 now upgraded). Source list of members from `GroupContextStore.membersSignal()`. Owner first, then members. `tbInfiniteScroll` calls `ManagementGroupsClient.getGroupMembers({id, afterId, take: 20})` and appends. Add-member button visible but `disabled` with `title="Coming soon"` tooltip (US-spec FR-010).

### Tests for User Story 4

- [ ] T058 [P] [US4] Integration test `GetResourceInstances_FirstPage_ReturnsAtMostTake` in `.../GetResourceInstancesEndpointTests.cs`. Seed 25 instances, assert first page returns 20 and a cursor.

- [ ] T059 [P] [US4] Integration test `GetResourceInstances_CursorPagination_NoDuplicatesAcrossPages` (same file).

- [ ] T060 [P] [US4] Integration test `GetResourceInstances_EmptyTrailingPage_ReturnsNullCursor` (same file).

- [ ] T061 [P] [US4] Integration test `GetGroupMembers_FirstPage_IncludesOwnerFirst` in `.../GetGroupMembersEndpointTests.cs`.

- [ ] T062 [P] [US4] Integration test `GetGroupMembers_CursorPagination_NoDuplicatesAcrossPages` (same file).

- [ ] T063 [P] [US4] Integration test `GetGroupMembers_NonMemberCaller_Returns403` (same file).

**Checkpoint**: User Story 4 shippable — both Resources and Members panels infinite-scroll cleanly.

---

## Phase 7: User Story 5 — Edit a group's name and description (Priority: P2)

**Goal**: Reuse `CreateOrEditGroupModalComponent` in edit mode; PUT endpoint already adds 409 (from US1 path via existing handler).

### Backend — US5

- [ ] T064 [US5] [FR-003] Mirror T019 logic into `UpdateManagementGroupHandler`: pre-check uniqueness `(OwnerId, normalizedName)` excluding the group being updated (`g.Id != command.Id`). Throw the same `GroupNameAlreadyTakenException` → 409.

### Frontend — US5

- [ ] T065 [US5] Add edit handling in `GroupHeaderPanelComponent`: clicking Edit emits `edit`; parent (`GroupDetailPageComponent`) opens `CreateOrEditGroupModalComponent` with `mode="edit"` and `initialValue={group}`. On success: refresh group in `GroupContextStore`, toast "Group updated".

### Tests for User Story 5

- [ ] T066 [P] [US5] Integration test `UpdateManagementGroup_DuplicateNameAmongOthers_Returns409` in `.../UpdateManagementGroupEndpointTests.cs`.

- [ ] T067 [P] [US5] Integration test `UpdateManagementGroup_SameNameAsItself_Succeeds` (same file) — confirms exclude-self.

**Checkpoint**: User Story 5 shippable — edit reuses Create flow.

---

## Phase 8: User Story 6 — Delete group with cascade (Priority: P2)

**Goal**: Click Delete in header → confirmation dialog with affected counts → on confirm soft-delete group, cascade-delete schemas/instances, redirect to dashboard.

### Backend — US6

- [ ] T068 [US6] [FR-004] Create `GroupDeletedHandler` in `backend/src/Modules/Resources/ThingsBooksy.Modules.Resources.Core/Features/GroupDeletedHandler.cs` (or extend the existing one if it already removes `GroupReadModel`). Subscribe to `GroupDeleted` event. In one Unit-of-Work: `UPDATE resource_instances SET DeletedAt = now() WHERE GroupId = @e.GroupId AND DeletedAt IS NULL`; `DELETE FROM resource_types WHERE GroupId = @e.GroupId CASCADE` (EF: load + RemoveRange + SaveChanges since FK to `ResourcePropertyDefinitions` cascades). Idempotent: zero matched rows is a no-op. Inbox de-dup is provided by infrastructure. Register handler in `Resources` module's `Extensions.cs` if not auto-discovered.

### Frontend — US6

- [ ] T069 [US6] Add delete handling in `GroupHeaderPanelComponent`: clicking Delete calls `ConfirmDialogService.confirm({ title: 'Delete this group?', message: `It will also delete ${schemaCount} schemas and ${resourceCount} resources.`, danger: true })`. Counts read from `GroupContextStore` (loaded counts; if there are more on the server, the count is approximate — acceptable). On confirm: call `ManagementGroupsClient.deleteManagementGroup(group.id)`, toast "Group deleted", `router.navigate(['/dashboard'])`.

### Tests for User Story 6

- [ ] T070 [P] [US6] Integration test `DeleteManagementGroup_CascadesToResourceInstancesAndTypes` in `.../DeleteManagementGroupEndpointTests.cs`. Seed a group with 2 schemas and 5 instances → DELETE → assert group `DeletedAt` set, all 5 instances `DeletedAt` set, both schemas hard-deleted. (Cascade is async via outbox/event-bus; test must await event delivery — use the existing test infrastructure pattern, presumably a helper that processes pending events synchronously inside the test.)

- [ ] T071 [P] [US6] Integration test `GroupDeletedHandler_RedeliveredEvent_IsNoOp` in `.../GroupDeletedHandlerTests.cs` (cross-module test — placed in Resources integration tests because it tests Resources subscriber behaviour). Seed group + cascade once → re-publish event → assert no exception, no further row changes.

**Checkpoint**: User Story 6 shippable — destructive flow with cascade and audit-friendly soft-deletes.

---

## Phase 9: User Story 7 — Delete schema with cascade (Priority: P3)

**Goal**: Delete a single schema; cascade soft-delete its instances; hard-delete the schema.

### Backend — US7

- [ ] T072 [US7] [FR-019] Extend `DeleteResourceTypeHandler` (existing) to cascade-soft-delete `ResourceInstance` rows of that type before hard-deleting the type. Currently the handler likely returns 400 if instances exist (per 009 handoff notes); switch to cascade. Domain check: caller must be group owner (existing). Wrap in single SaveChanges.

### Frontend — US7

- [ ] T073 [US7] Add delete-per-row action in `SchemasPanelComponent`: hover row reveals a trash icon (owner-only). Click → `ConfirmDialogService.confirm({ title: `Delete schema '${schema.name}'?`, message: `${instanceCount} resources of this type will also be deleted.`, danger: true })`. Instance count computed locally by filtering `GroupContextStore.resourcesSignal().items.filter(r => r.resourceTypeId === schema.id).length` (approximate but acceptable). On confirm: `ResourcesClient.deleteResourceType(schema.id)`, toast "Schema deleted", remove from `schemasSignal`, remove matching resources from `resourcesSignal`.

### Tests for User Story 7

- [ ] T074 [P] [US7] Integration test `DeleteResourceType_CascadesToInstances` in `.../DeleteResourceTypeEndpointTests.cs`. Seed schema + 3 instances → DELETE → assert schema hard-deleted, 3 instances soft-deleted (DeletedAt set).

**Checkpoint**: User Story 7 shippable — schemas removable cleanly.

---

## Phase 10: User Story 8 — Notifications consistency check (Priority: P2)

**Goal**: All deliverables already implemented in Phase 2 Foundational; this phase verifies behaviour across all stories.

### Verification — US8

- [ ] T075 [US8] Audit every API call site introduced in stories 1–7 to confirm: (a) success path calls `NotificationService.success(...)` exactly once with a meaningful message; (b) error path is handled by `errorInterceptor` automatically (no manual `.error(...)` calls anywhere except in interceptor). Walk through the Quickstart §1–§15 manually; record any missing toasts.

- [ ] T076 [US8] Confirm the `x-silent-errors: true` header is present on `ManagementGroupsClient.nameAvailable(...)` calls so a transient 5xx during availability check doesn't spam toasts while typing.

- [ ] T077 [US8] Confirm `prefers-reduced-motion` reduces toast animation to ~0ms; visual check.

**Checkpoint**: User Story 8 shippable — feedback consistent across all flows.

---

## Phase 11: Polish & Cross-Cutting Concerns

- [ ] T078 Run `architecture-guard` agent on the Wave to catch cross-module violations (direct refs, orphaned events, missing module registrations, duplicate schemas, missing InternalsVisibleTo). Per orchestration in CLAUDE.md.

- [ ] T079 Regenerate frontend API client by running `fe-api-client-writer` agent. New endpoints (`name-available`, paginated members) and amended `GetResourceInstances` envelope must be reflected in `frontend/src/app/api/ManagementGroups.ts`, `frontend/src/app/api/Resources.ts`, and `frontend/src/app/api/data-contracts.ts`.

- [ ] T080 Run `html-extractor` on each design HTML (`ThingsBooksy Group Detail.html`, `ThingsBooksy Schema Designer.html`, `create-group-modal.jsx`+`ThingsBooksy Dashboard.html` for modal). Approve Phase 6 outputs; gate downstream FE wave with `fe-plan-validator`.

- [ ] T081 [P] Manual quickstart.md verification end-to-end. Sign in → §1 through §15. Record failures.

- [ ] T082 [P] Manual negative-path verification per quickstart.md. Duplicate names, deleted-schema race, server-down, reduced-motion, non-owner viewer.

- [ ] T083 Run `dotnet format backend/ThingsBooksy.slnx` to enforce formatting per constitution §VIII.

- [ ] T084 Run `cd frontend && npm run lint && npm run build` to confirm zero lint errors and zero build errors.

- [ ] T085 Commit changes with conventional message per project history. Use `/speckit-git-commit` skill (the optional hook).

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)** → no deps.
- **Phase 2 (Foundational)** → after Phase 1. **Blocks all user-story phases.**
- **Phase 3 (US1)** → after Phase 2.
- **Phase 4 (US2)** → after Phase 2 (independent of US1 in BE; FE consumes navigation from US1 only at runtime).
- **Phase 5 (US3)** → after Phase 4 (Create Resource modal renders dynamic fields per a schema; needs schemas creatable). FE-wise independent if schemas can be seeded manually for testing.
- **Phase 6 (US4)** → after Phase 5 (FE infinite scroll lives in Resources and Members panels which exist by US3).
- **Phase 7 (US5)** → after Phase 3 (reuses the Create modal).
- **Phase 8 (US6)** → after Phase 4 (cascade subscriber requires schemas exist for end-to-end test).
- **Phase 9 (US7)** → after Phase 4.
- **Phase 10 (US8)** → after Phases 3–9 (verification step).
- **Phase 11 (Polish)** → after Phase 10.

### Parallel Opportunities

- T003 (config) and T004+T005 (config) can run in parallel (different modules / different files).
- T008–T015 (FE shared primitives) can mostly run in parallel — all are independent files.
- Within US1, T028–T034 (tests) all run in parallel.
- Within US2, T037–T041 (FE components) and T044–T047 (tests) mostly independent.
- Within US4, T058–T063 are independent integration tests.
- US5 (T064–T067) and US6 (T068–T071) and US7 (T072–T074) can be developed in parallel once Phase 4 completes.

### Within Each User Story

- BE handlers/queries before BE endpoints before BE integration tests.
- FE components can start in parallel; the route wiring is the final FE step per story.
- Quality gates per the orchestration in CLAUDE.md: `module-writer` → `migration-agent` (if schema changed) → `quality-reviewer` (interactive) → `integration-test-writer`. Tests inside each story phase above are *produced* by `integration-test-writer`, listed here for traceability.

---

## Parallel Example: User Story 1 BE + FE

```text
# BE wave A (parallel):
Task T003 Configure unique partial index on ManagementGroup
Task T017 Add IsGroupNameAvailableQuery/Handler/Result
Task T019 Add uniqueness pre-check to CreateManagementGroupHandler

# FE wave (after T079 regen of API client):
Task T020 CreateOrEditGroupModalComponent
Task T022 GroupContextStore
Task T024 GroupHeaderPanelComponent
Task T025 Stub panels

# Tests wave (after BE wave + integration-test-writer agent invocation):
Task T028, T029, T030, T031 IsGroupNameAvailable tests
Task T032, T033, T034 CreateManagementGroup uniqueness tests
```

---

## Implementation Strategy

### MVP First (User Story 1 only)

1. Complete Phase 1 (Setup) and Phase 2 (Foundational).
2. Complete Phase 3 (US1) including BE tests via `integration-test-writer`.
3. **Stop, validate Quickstart §1–§4.** Deploy/demo.

### Incremental Delivery

- Foundational → MVP via US1 (create + detail page populated empty).
- + US2 → schema creation works end-to-end.
- + US3 → first real resources land.
- + US4 → scalable browsing.
- + US5 → editable groups.
- + US6, US7 → safe destruction with cascade.
- US8 verified.
- Polish.

### Agent Pipeline Dispatch (orchestrated from the main session, per CLAUDE.md)

After tasks.md is approved by `plan-validator`:

1. `module-writer` × 2 in parallel:
   - **ManagementGroups**: T003, T017, T018, T019, T019b (MemberCount), T054, T055, T064.
   - **Resources**: T004, T005, T035, T036, T051, T052, T053, T068, T072.
   Pass task IDs explicitly.
2. `migration-agent` × 2 in parallel after each module-writer reports `Schema changes != NONE` (T006 for ManagementGroups, T007 for Resources).
3. `quality-reviewer` × 2 (interactive, sequential) — block `integration-test-writer` until ended.
4. `integration-test-writer` × 2 in parallel (writes tasks T028–T034, T044–T047, T058–T063, T066–T067, T070–T071, T074).
5. `architecture-guard` (T078) after both modules' integration-test-writer report COMPLETE.
6. `fe-api-client-writer` (T079).
7. `html-extractor` × 3 + `fe-plan-validator` per HTML (T080).
8. `fe-component-writer` × N in parallel grouped by feature (`features/dashboard` for modal upgrade, `features/groups` for panels + create-resource modal, `features/schemas` for designer page; plus shared primitives if not already in foundational).
9. `fe-route-writer` × 2 (`features/groups`, `features/schemas`).
10. Manual quickstart (T081, T082) + format/lint/build (T083, T084) + commit (T085).
