# Phase 0 Research: Group Detail & Schema Designer

**Date**: 2026-05-14
**Status**: Complete — all unknowns resolved during the planning session and locked into ADR-002. This document records the resolved decisions in the standard *Decision / Rationale / Alternatives considered* shape so downstream agents have a single reference.

---

## 1. Cursor pagination semantics

**Decision**: Pagination shape across all paginated lists in this feature is `?afterId=<guid?>&take=<int=20>` returning `{ items: T[], nextCursor: Guid? }`. Ordering: `ORDER BY Id` ascending (GUID v7 is monotonically increasing per insertion time). `nextCursor` is the `Id` of the last item in `items` whenever the page is full (`items.Count == take`); otherwise `null`. Empty result with non-null cursor input is a valid end-of-stream signal.

**Rationale**: GUID v7 already mandated by the constitution gives stable ordering without an extra `CreatedAt` index. Cursor-based avoids offset duplication under concurrent writes (ADR-002 §1). Single shape across endpoints means one FE primitive (`tb-infinite-scroll`) and one BE pattern.

**Alternatives considered**:
- *Offset/limit*: rejected — duplicates/skips under concurrent inserts; ADR-002 §1 explicitly forbids mixing strategies.
- *Keyset on `(CreatedAt, Id)`*: equivalent in this codebase because `Id` is time-ordered; the second column is redundant.
- *Page tokens (opaque)*: overkill for the scale and obscures debugging.

---

## 2. Cross-module cascade strategy

**Decision**: Group deletion publishes the existing `GroupDeleted(Guid GroupId)` event. The Resources module implements `GroupDeletedHandler : IEventHandler<GroupDeleted>` which, in one transaction, soft-deletes all `ResourceInstance` rows where `GroupId == event.GroupId AND DeletedAt IS NULL` and hard-deletes all `ResourceType` rows where `GroupId == event.GroupId`. The handler is **idempotent**: replaying it on an already-cleaned group is a no-op (zero rows matched). Outbox/inbox infrastructure already exists in `ResourcesDbContext` (`Inbox` DbSet) and ensures at-least-once delivery without duplicate effects when paired with idempotent handlers.

**Rationale**: Honors event-driven decoupling from ADR-001 and ADR-002 §2. Closes ADR-001's open issue on cascade behaviour.

**Alternatives considered**:
- *`IModuleClient` direct call from ManagementGroups*: rejected — couples ManagementGroups to Resources' lifecycle rules and bypasses outbox guarantees.
- *Soft-delete `ResourceType` too*: rejected — schema is a definition, not user data; recoverable by recreation. Soft-deleting types just adds query-filter noise.
- *Hard-delete `ResourceInstance` too*: rejected — instances are user data with EAV property values; soft-delete keeps audit trail.

---

## 3. Uniqueness enforcement layering

**Decision**: Three layers from outermost to innermost.

1. **Async client validator** (Reactive Forms async validator) calling `GET /management-groups/name-available?name=…` with `timer(300) + first()` debounce. Surfaces inline error in modal *before* submit. **Only present for the group-name modal** — schema uniqueness is checked synchronously on FE against the in-memory schema list from the active group context.
2. **Handler-level pre-check** in `CreateManagementGroupHandler`, `UpdateManagementGroupHandler`, `CreateResourceTypeHandler`, `UpdateResourceTypeHandler`. Reads the same-scope existing names through the handler's data provider and throws a domain exception (`GroupNameAlreadyTakenException`, `ResourceTypeNameAlreadyExistsException`) when collision found. Exception is mapped to HTTP 409 by the existing exception handler middleware.
3. **DB unique index** as last line of defence:
   - `(OwnerId, Name)` on `management_groups.management_groups`, filtered `WHERE "DeletedAt" IS NULL` so re-creating a name after delete is allowed.
   - `(GroupId, Name)` on `resources.resource_types`, no filter (types are hard-deleted on group delete; no soft-delete column).

**Rationale**: Defence in depth. UX feedback comes from layer 1; predictable HTTP semantics from layer 2; data integrity guarantee from layer 3.

**Alternatives considered**:
- *DB index only*: rejected — UX would surface `DbUpdateException` translated to generic 500 unless mapped manually for every endpoint.
- *Handler check only*: rejected — race between two parallel requests could both pass the check and corrupt invariant.
- *Async client validator for schemas too*: rejected — schemas list is already loaded into FE context for the detail page; synchronous is faster and avoids a chatty endpoint.

---

## 4. Drag-and-drop field reorder on Angular

**Decision**: `@angular/cdk/drag-drop` — specifically `cdkDropList` + `cdkDrag` directives with `cdkDragHandle` on the handle icon. Persist new order locally in the form state; emit a single signal change when `cdkDropListDropped` fires. The package is added to `package.json` as `@angular/cdk` (matches Angular 21 major version).

**Rationale**: Mature, accessible (keyboard reorder built-in), ships in the Angular ecosystem (no third-party risk), supports `move-by-keyboard` for `prefers-reduced-motion` users. Implements `Move item up/down with arrow keys` accessibility primitive without custom code.

**Alternatives considered**:
- *Custom HTML5 drag events*: rejected — reinvents accessibility, focus management, and visual feedback the CDK already provides.
- *SortableJS via wrapper*: rejected — third-party dependency, breaks Angular signal lifecycle.

---

## 5. Infinite-scroll trigger strategy

**Decision**: An Angular structural directive `tb-infinite-scroll` (selector `[tbInfiniteScroll]`) places a sentinel element at the end of the list. The directive uses `IntersectionObserver` with `{ root: null, rootMargin: '120px', threshold: 0 }`. When the sentinel becomes intersecting, the directive emits `(loadMore)`. Throttled by an internal `loading` boolean (caller passes it back via input) to avoid duplicate emissions.

**Rationale**: Native browser API, performant (browser-level batching), no scroll handler noise, decoupled from any specific scroll container. Works for both `Resources` and `Members` panels (separate observer instances per directive instance).

**Alternatives considered**:
- *Scroll-event listener with `(scrolled)` calc*: rejected — verbose, brittle on rebound on touch devices, runs in `NgZone` causing change-detection thrash.
- *Angular CDK Virtual Scroll*: deferred to a future iteration — provides recycling but requires fixed row heights and conflicts with this feature's variable-height rows. Spec calls out specifically *not* to use virtual scroll.

---

## 6. Toast / notification architecture

**Decision**: `NotificationService` is a singleton service exposing a public `signal<Toast[]>` queue plus methods `success(msg)`, `error(msg)`, `info(msg)`, `dismiss(id)`. Each call enqueues a `Toast { id, kind, message, createdAt }` and schedules dismissal via `effect()` + `setTimeout(5000)`. Maximum 3 simultaneous toasts (oldest evicted FIFO). The `tb-toast` component subscribes to the signal and renders the list with `fadeUp` enter / `fadeOut` leave animations. Reduced-motion shortens animations to ~0 ms.

The existing `errorInterceptor` injects `NotificationService` and calls `.error(...)` on any 4xx/5xx, using `error.detail || error.title || statusText` as the message. Domain 409 collisions are silently swallowed *only* when an explicit `silentOn` header is present on the original request (used by name-availability check to avoid double-toasting); otherwise toasted normally.

**Rationale**: Zero new dependencies, plays well with signals, integrates one-call into the existing interceptor. Stack cap prevents spam if a loop errors repeatedly.

**Alternatives considered**:
- *Material Snackbar*: rejected — adds Angular Material (large dep, conflicting design language).
- *Angular CDK Overlay*: rejected — overkill for a corner toast stack; we don't need positioning strategies.
- *No service, inline banners*: rejected — can't survive route changes (success toast after Create Group fires on the *next* page).

---

## 7. Schema live-preview reactivity

**Decision**: The Schema Designer holds three pieces of state in `schema-designer-page` component:

- `name = signal('')` and `description = signal('')` two-way bound to inputs.
- `fields = signal<FieldDraft[]>([])` — manipulated through `addField()`, `removeField(id)`, `reorderFields(prev, next)`, `updateField(id, patch)`.

The preview panel is a pure component receiving `name`, `description`, `fields` as `input()`s; renders deterministically. Drag-reorder fires `reorderFields(...)` which calls `moveItemInArray()` from CDK. Marking dirty: a separate `dirty = computed(() => !equalsInitial(...))` flag drives the draft badge and `CanDeactivate` guard.

**Rationale**: Signals provide synchronous-feeling updates with automatic OnPush re-render. No RxJS subscription churn. `computed()` for `dirty` keeps derivation cheap.

**Alternatives considered**:
- *Reactive Forms valueChanges → toSignal*: rejected — Reactive Forms add ceremony (FormGroup, FormArray, typed controls) without benefit here; field list is dynamic and we want optimistic updates without form revalidation flicker. Note: The Create Group modal *does* use Reactive Forms because it has fixed shape and benefits from validators/async-validators; Schema Designer uses signals because field list is dynamic.
- *NgRx*: rejected — gross overkill for component-local state.

---

## 8. CanDeactivate guard for unsaved schema changes

**Decision**: A `CanDeactivateFn` checks `component.dirty()` and, if truthy, calls the `ConfirmDialogService.confirm(...)` (a thin wrapper that opens `tb-confirm-dialog`) returning `Observable<boolean>`. Browser refresh / tab-close handled by registering `window.beforeunload` while `dirty()` is true; cleaned up on `OnDestroy`.

**Rationale**: Two paths to leave (Angular nav vs. browser-level nav) require two different mechanisms. Both gated by the same `dirty()` signal so the user sees consistent behaviour.

**Alternatives considered**:
- *`@HostListener('window:beforeunload')`*: same effect but less explicit lifecycle.
- *No browser-level guard*: rejected — F5 dataloss is exactly what the spec calls out as Edge Case.

---

## 9. Deterministic avatar color

**Decision**: Hash the group's `Id` (string form) with FNV-1a 32-bit, modulo 3 to pick a token name:

- 0 → `--color-accent-primary` (lavender)
- 1 → `--color-accent-secondary` (mint)
- 2 → `--color-accent-tertiary` (peach)

Exposed by `tb-avatar` component which accepts `id` and `initials` inputs and computes the colour internally. Future migration: when an optional `AccentColor` field is added to the entity, `tb-avatar` accepts an optional `color` input that overrides the hash — backwards-compatible.

**Rationale**: Fastest possible deterministic hash with zero allocations, no crypto need, three uniform buckets. Same input → same output across all views (dashboard, header, breadcrumb).

**Alternatives considered**:
- *Use existing `crypto.subtle.digest`*: rejected — async, overkill for non-security mapping.
- *Random per render*: rejected — explicitly violates "same group, same color everywhere" requirement.

---

## 10. EF unique-index with filter for soft-deleted rows (ManagementGroup)

**Decision**: In `ManagementGroupConfiguration`, configure the unique index as:

```csharp
builder.HasIndex(g => new { g.OwnerId, g.Name })
       .IsUnique()
       .HasFilter("\"DeletedAt\" IS NULL");
```

PostgreSQL partial index respects the quoted identifier per Npgsql conventions. Recreating a group with the same name after the first one is soft-deleted is allowed.

**Rationale**: Lets users re-use names of groups they previously deleted (very common UX expectation).

**Alternatives considered**:
- *No filter*: blocks name reuse forever after first delete — confusing UX.
- *Hard-delete groups instead of soft-delete*: would simplify the index but violates the existing soft-delete pattern and breaks `POST /management-groups/{id}/restore`.

---

## 11. Resource Types unique index — no filter

**Decision**: In `ResourceTypeConfiguration`, configure:

```csharp
builder.HasIndex(r => new { r.GroupId, r.Name }).IsUnique();
```

No filter clause because `ResourceType` has no soft-delete column (types are hard-deleted, including by cascade — see §2). Reuse of names after hard-delete works naturally because the index has no stale rows.

**Rationale**: Simpler than partial index. Matches `ResourceType`'s lifecycle.

**Alternatives considered**: none — straightforward.

---

## 12. "Available (mocked)" status badge

**Decision**: Pure FE label. No new BE field. The `tb-status-badge` component (already exists in shared/components) is invoked with the literal label `"Available (mocked)"` and the `available` variant token. A `// TODO: real status when ResourceInstance.Status lands` comment placed in the one render site.

**Rationale**: Spec explicitly out-of-scopes real status. The hard-coded "mocked" suffix telegraphs the temporary nature.

---

## 13. Outstanding considerations

None — all NEEDS CLARIFICATION resolved. The single forward-looking note is that *resource detail / edit / delete UI* is deferred; if a future iteration needs it, expect cursor pagination response shape and event handlers to remain stable.
