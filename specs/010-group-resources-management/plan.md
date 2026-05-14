# Implementation Plan: Group Detail & Schema Designer & Group/Resource Modals

**Branch**: `010-group-resources-management` | **Date**: 2026-05-14 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/010-group-resources-management/spec.md`
**ADRs honored**: [ADR-001-resource-management](../../.specify/decisions/ADR-001-resource-management.md), [ADR-002-cross-cutting-conventions](../../.specify/decisions/ADR-002-cross-cutting-conventions.md)

## Summary

Deliver the end-to-end "owner manages a group's catalog" flow on top of the existing `ManagementGroups` and `Resources` modules: a new `/groups/:groupId` detail page composed of four panels (header, schemas, resources, members), a new full-page Schema Designer at `/groups/:groupId/schemas/:id|new` with live form preview and drag-and-drop field reordering, and modals for creating/editing groups and creating resource instances. Backend gains cursor-based pagination, two unique constraints, a new `name-available` query endpoint, and a `GroupDeleted` cascade subscriber. Frontend introduces two new lazy feature routes (`features/groups`, `features/schemas`) and five reusable shared primitives (notifications, confirm dialog, avatar, type pill, infinite-scroll directive).

## Technical Context

**Language/Version**: C# 13 (.NET 10) for backend; TypeScript 5.9 (Angular 21) for frontend.
**Primary Dependencies**: ASP.NET Core 10 Minimal API, EF Core 10, Serilog, Swashbuckle (BE); Angular 21 standalone, RxJS 7.8, `@angular/cdk/drag-drop` (FE — added in this feature), `@angular/forms` Reactive Forms.
**Storage**: PostgreSQL 17, separate schema per module (`management_groups`, `resources`).
**Testing**: xUnit + WebApplicationFactory (`ThingsBooksyWebAppFactory` applies migrations automatically) for backend integration tests; Vitest + Angular TestBed for frontend unit tests.
**Target Platform**: Single deployable .NET 10 web app + Angular 21 SPA bundled via Docker; primary client = evergreen desktop browser, responsive down to tablet (≥ 768 px); narrow phone is best-effort.
**Project Type**: Web app — modular monolith backend + Angular frontend (already in `backend/` and `frontend/`).
**Performance Goals**: Group detail page initial paint < 1 s on local dev DB; infinite scroll fetches < 300 ms p95 for `?take=20`. Animation work respects `prefers-reduced-motion`.
**Constraints**: GUID v7 mandatory; no direct cross-module references; commands constructed in endpoints only; handlers depend on `IDataProvider` not `DbContext`; schema isolation per module.
**Scale/Scope**: Single-tenant developer-facing app; expected per-group sizes — < 100 schemas (typical 1–10), < 5 000 resources (typical 10–500), < 200 members (typical 5–30). Page-size for pagination = 20.

## Constitution Check

Gates derived from `.specify/memory/constitution.md` (v1.2.0). All gates evaluated against this plan; results below.

| Gate | Verdict | Note |
|---|---|---|
| I. Modular Monolith — no direct cross-module refs | PASS | Cascade delete uses existing `GroupDeleted` event (ADR-002 §2). No `ManagementGroups` ↔ `Resources` direct refs. No new shared types beyond what already exists in `Shared.Abstractions`. |
| II. Simplified DDD — `.Api` + `.Core` per module | PASS | Both touched modules already follow this structure; no new modules. |
| III. Minimal API Endpoints | PASS | New endpoints (`GET /management-groups/name-available`, `GET /management-groups/{id}/members`) registered via `Expose()` in `ManagementGroupsModule.cs`. Resources endpoints amended in place. |
| IV. Inter-Module Communication via Events | PASS | Cascade is event-driven (`GroupDeleted` already published). New `GroupDeletedHandler` (or extension thereof) in Resources module. |
| V. Test-First | PASS | Integration tests written per `integration-test-writer` agent for: paginated list endpoints, uniqueness constraints (409), `name-available`, cascade cleanup. Domain entity changes also gain unit tests. |
| VI. Persistence & Migrations | PASS | Two new migrations: one per module. Schemas isolated (`management_groups`, `resources`). |
| VII. Simplicity / YAGNI | PASS | No new frameworks. `@angular/cdk/drag-drop` is the only new FE dependency — it ships with Angular CDK already in the ecosystem and avoids reinventing drag handlers. No MediatR / AutoMapper / heavy libs added. |
| VIII. Code Formatting | PASS | All BE diffs run `dotnet format`; FE diffs run Prettier via existing scripts. |
| IX. Domain Entities — Encapsulation | PASS | `ManagementGroup` and `ResourceType` already follow the pattern; new updates extend `Update` methods that accept command objects. No new entities introduced. |
| X. Identifiers — GUID v7 | PASS | All new IDs (none beyond migrations' index keys) use `Guid.CreateVersion7()`. Cursor is the existing GUID v7 of the last seen row — sorts monotonically. |
| XI. DataProvider Pattern | PASS | New handlers add `IXxxDataProvider` interfaces in `Features/{Feature}/DataProviders/`. `AddDataProviders` already wired per module. |
| XII. Command Construction in Endpoints | PASS | Two new request DTO records (`GetMembersRequest`, query strings actually — only `MembersPageQuery` is a query record; `name-available` uses query string param). Commands for any new write paths are constructed in the endpoint lambda. |
| XIII. Naming | PASS | `GetGroupMembersQuery` / `GetGroupMembersQueryHandler` / `GetGroupMembersResult`; `IsGroupNameAvailableQuery` / `…Handler` / `…Result`; `GetResourceInstancesPagedQuery` (amends existing). |
| XIV. Internals Visibility | PASS | No new `.Core` projects, so no new `InternalsVisibleTo` files. |

**Result**: All gates PASS. No Complexity Tracking entries required.

## Project Structure

### Documentation (this feature)

```text
specs/010-group-resources-management/
├── plan.md                  # This file
├── spec.md                  # Source spec (already written)
├── research.md              # Phase 0 output — see below
├── data-model.md            # Phase 1 output — see below
├── contracts/               # Phase 1 output — see below
│   ├── backend-endpoints.md
│   └── shared-events.md
├── quickstart.md            # Phase 1 output — end-to-end happy path
├── checklists/
│   └── requirements.md      # Already written
└── tasks.md                 # Phase 2 — produced by /speckit-tasks (NOT here)
```

### Source code (repository)

**Backend** — *only* the files this feature touches; everything else is reused as-is.

```text
backend/src/
├── Modules/
│   ├── ManagementGroups/
│   │   ├── ThingsBooksy.Modules.ManagementGroups.Api/
│   │   │   ├── ManagementGroupsModule.cs                          # + Expose() for new endpoints
│   │   │   └── Requests/
│   │   │       └── (existing CreateManagementGroupRequest etc.)
│   │   ├── ThingsBooksy.Modules.ManagementGroups.Core/
│   │   │   ├── Domain/ManagementGroup.cs                          # no behavioral change
│   │   │   ├── Features/
│   │   │   │   ├── CreateManagementGroup/                         # + uniqueness validation
│   │   │   │   ├── UpdateManagementGroup/                         # + uniqueness validation
│   │   │   │   ├── DeleteManagementGroup/                         # no change (publishes GroupDeleted already)
│   │   │   │   ├── GetGroupMembers/                               # NEW (paginated query)
│   │   │   │   └── IsGroupNameAvailable/                          # NEW (query)
│   │   │   └── DAL/ManagementGroupsDbContext.cs                   # + unique index config
│   │   └── ThingsBooksy.Modules.ManagementGroups.Migrations/
│   │       └── Migrations/{date}_AddGroupOwnerNameUniqueIndex.cs  # NEW migration
│   └── Resources/
│       ├── ThingsBooksy.Modules.Resources.Api/
│       │   └── ResourcesModule.cs                                 # update GetResourceInstances endpoint signature
│       ├── ThingsBooksy.Modules.Resources.Core/
│       │   ├── Domain/ResourceType.cs                             # no behavioral change
│       │   ├── Features/
│       │   │   ├── CreateResourceType/                            # + uniqueness validation
│       │   │   ├── UpdateResourceType/                            # + uniqueness validation
│       │   │   ├── GetResourceInstances/                          # cursor pagination params
│       │   │   └── GroupDeletedHandler.cs                         # NEW (cascade)
│       │   └── DAL/ResourcesDbContext.cs                          # + unique index config
│       └── ThingsBooksy.Modules.Resources.Migrations/
│           └── Migrations/{date}_AddResourceTypeUniqueAndCursorIndexes.cs   # NEW migration
└── Shared/
    └── ThingsBooksy.Shared.Abstractions/
        └── Events/ManagementGroups/GroupDeleted.cs                # NO CHANGE — already exists
```

**Frontend** — new feature folders + reused shared primitives.

```text
frontend/src/app/
├── api/                                          # Regenerated by fe-api-client-writer (post-BE)
│   ├── ManagementGroups.ts                       # + nameAvailable, paginated members
│   ├── Resources.ts                              # cursor params on getInstances
│   └── data-contracts.ts                         # cursor envelope, MembersResult etc.
├── app.routes.ts                                 # register new lazy routes
├── core/
│   └── interceptors/error.interceptor.ts         # extend with NotificationService injection
├── features/
│   ├── dashboard/
│   │   ├── create-group-modal/                   # REWRITE placeholder → CreateOrEditGroupModalComponent
│   │   ├── dashboard-admin-panel/                # rows become clickable (router link), use tb-avatar
│   │   └── dashboard-page/                       # refresh group list after Create
│   ├── groups/                                   # NEW feature
│   │   ├── groups.routes.ts                      # exports groupsRoutes
│   │   ├── group-detail-page/                    # container of 4 panels
│   │   ├── group-header-panel/
│   │   ├── schemas-panel/
│   │   ├── resources-panel/
│   │   ├── members-panel/
│   │   ├── create-resource-modal/                # standalone modal
│   │   └── group-context.store.ts                # signal store holding current group + schemas + members + resources cursor
│   └── schemas/                                  # NEW feature
│       ├── schemas.routes.ts                     # exports schemasRoutes
│       ├── schema-designer-page/                 # two-column layout with CanDeactivate guard
│       ├── schema-form-panel/
│       ├── schema-preview-panel/
│       ├── field-row/
│       └── guards/unsaved-changes.guard.ts       # CanDeactivate
├── shared/
│   ├── components/
│   │   ├── modal/                                # EXISTS, reused
│   │   ├── toast/                                # NEW (tb-toast)
│   │   ├── confirm-dialog/                       # NEW (tb-confirm-dialog)
│   │   ├── avatar/                               # NEW (tb-avatar)
│   │   └── type-pill/                            # NEW (tb-type-pill)
│   ├── directives/
│   │   └── infinite-scroll.directive.ts          # NEW (tb-infinite-scroll, IntersectionObserver)
│   └── services/
│       └── notification.service.ts               # NEW (signal queue, success/error/info)
└── styles/
    ├── _tokens.scss                              # NO CHANGE
    └── _animations.scss                          # NEW — fadeUp, fadeDown, modalEnter, draftPulse keyframes
```

**Structure Decision**: Backend follows the existing modular monolith layout — no new modules, only feature folders inside the two existing `.Core` projects (one feature per command/query group). Frontend introduces two new feature folders (`features/groups`, `features/schemas`) following `angular-folder-structure.md`; existing `features/dashboard` is edited in place. Shared FE primitives go to `shared/components` and `shared/services` per convention. The Bootstrapper, solution file, and module registration are unchanged.

## Phase 0 — Outline & Research

Output: `research.md` (sibling to this file). Because every architectural unknown has already been resolved during the planning session (16 decisions in the approved plan file) and locked into ADR-002, Phase 0 documents the resolved decisions in research format rather than discovering new ones. No `NEEDS CLARIFICATION` markers remain.

Topics covered in `research.md`:

1. Cursor pagination semantics (cursor type, ordering, empty-tail signal).
2. Cross-module cascade strategy (event subscriber idempotency, transaction boundary).
3. Uniqueness enforcement layering (DB unique index, handler-level pre-check, async client-side check).
4. Drag-and-drop reorder approach on Angular (`@angular/cdk/drag-drop` vs custom; pick CDK).
5. Infinite scroll trigger strategy (`IntersectionObserver` vs scroll event; pick IntersectionObserver).
6. Toast / notification architecture (signal store, error interceptor integration, stack limits).
7. Schema "live preview" reactivity (deep signal vs RxJS form valueChanges; pick `toSignal(form.valueChanges)` + manual ordering signal).
8. CanDeactivate guard for unsaved schema changes (form `.dirty` flag + Angular `CanDeactivateFn`).
9. Deterministic avatar color derivation (FNV-1a hash modulo 3 → token name; documented in research).
10. EF unique-index filter for soft-deleted rows on ManagementGroup (`HasFilter("\"DeletedAt\" IS NULL")`).

## Phase 1 — Design & Contracts

### `data-model.md`

Documents all entity changes (none in this feature beyond constraints) and all DTOs / result records introduced or modified. Generated below.

### `contracts/backend-endpoints.md`

Lists every HTTP endpoint added or modified, with method, path, request shape, response shape, status codes, and an example. Modules in scope:

- `GET /management-groups/name-available?name={name}` (NEW) — returns `{ available: bool }`. 200 OK always (no 404).
- `GET /management-groups/{id}/members?afterId={guid?}&take={int=20}` (NEW) — returns `{ items: GroupMember[], nextCursor: Guid? }`. 200 OK; 403 if not member; 404 if group missing.
- `POST /management-groups` (AMEND) — adds 409 response when `(OwnerId, Name)` collides.
- `PUT /management-groups/{id}` (AMEND) — adds 409 response when `(OwnerId, Name)` collides.
- `POST /resources/types` (AMEND) — adds 409 response when `(GroupId, Name)` collides.
- `PUT /resources/types/{id}` (AMEND) — adds 409 response when `(GroupId, Name)` collides.
- `GET /resources/instances?groupId={guid}&resourceTypeId={guid?}&afterId={guid?}&take={int=20}` (AMEND) — returns `{ items: ResourceInstance[], nextCursor: Guid? }` (was a flat list). Backward-incompatible response shape but all FE callers will be regenerated.

### `contracts/shared-events.md`

Single existing event reused: `GroupDeleted(GroupId)` in `Shared.Abstractions/Events/ManagementGroups/GroupDeleted.cs`. No new contracts.

### `quickstart.md`

End-to-end manual verification script: sign-up → sign-in → dashboard → Create new group → land on `/groups/:id` → Add schema → Schema Designer → add fields → drag-reorder → Save → return to detail → Add resource → assert in Resources list → infinite scroll → Edit group → Delete group with cascade.

### Agent context update

Update the `<!-- SPECKIT START -->` / `<!-- SPECKIT END -->` markers in `CLAUDE.md` to point to this plan file.

## Phase 2 — Tasks (handed to `/speckit-tasks`)

`/speckit-tasks` will dependency-order the work. Expected high-level shape:

- **Wave A (Backend, parallelizable)** — module-writer × 2:
  - ManagementGroups: 2 new queries (`GetGroupMembers`, `IsGroupNameAvailable`), 2 amended handlers (Create/Update validation), 1 DbContext config update, 1 migration, 2 new endpoints in `Expose()`.
  - Resources: cursor pagination on `GetResourceInstances`, uniqueness validation on `CreateResourceType` + `UpdateResourceType`, 1 DbContext config update, 1 migration, 1 new `GroupDeletedHandler`.
- **Wave A tail** — `migration-agent` × 2 → `quality-reviewer` × 2 (interactive, sequential) → `integration-test-writer` × 2 → `architecture-guard`.
- **Wave B (Frontend, after `fe-api-client-writer` regen)** — `html-extractor` per design HTML → `fe-plan-validator` per page → `fe-component-writer` × N parallelized by feature, then `fe-route-writer` × 2 (`groups`, `schemas`).
- **Wave B includes** the new shared primitives (`tb-toast`, `tb-confirm-dialog`, `tb-avatar`, `tb-type-pill`, `tb-infinite-scroll`) and `NotificationService` — these are dependencies for the four feature components and ship in the same wave as separate `fe-component-writer` invocations early in the wave.

## Complexity Tracking

No constitution violations — section intentionally empty.
