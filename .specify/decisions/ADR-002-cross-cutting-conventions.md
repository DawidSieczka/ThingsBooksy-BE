# ADR-002: Cross-Cutting Conventions
**Status:** accepted
**Date:** 2026-05-14
**Feature:** cross-cutting-conventions

## Context
Feature 010 (Group Detail, Schema Designer, resource management) spans both backend and frontend with patterns that will repeat in future features: pagination at scale, cascade semantics, uniqueness validation, deterministic visual identity, and user-facing notifications. These decisions establish project-wide conventions rather than feature-specific choices, and formalize open issues left by ADR-001.

## Decision 1 — Cursor-Based Pagination as Project Idiom

**Decision**: Resources and Members lists use cursor-based pagination (`?afterId=<guid>&take=20`) returning `{ items: [...], nextCursor: Guid? }`. This becomes the canonical pagination pattern for all forward-only infinite-scroll lists in the project.

**Rationale**:
- Stable under concurrent inserts/deletes: same cursor always yields the same logical position, preventing duplicates and skipped rows.
- Index-efficient: `WHERE Id > cursor ORDER BY Id LIMIT take` leverages B-tree seeks, no offset cost.
- Fits infinite scroll natively: client stores only the last ID, not page numbers.
- No alternative pagination strategy (offset-limit, keyset) will be mixed into the same codebase.

**Consequences**:
- Clients must maintain cursor state; no random-access jumps to arbitrary pages (acceptable for infinite scroll UX).
- Requires monotonic ID ordering (GUID v7 insertion order); ADR-001 already mandates GUID v7.
- All list endpoints returning paginated data must adopt this shape; future deviation requires an amendment.

---

## Decision 2 — Cascade Delete via Domain Event (GroupDeleted)

**Decision**: When a group is deleted, the ManagementGroups module publishes `GroupDeleted` event. The Resources module subscribes and performs cascading cleanup: soft-delete all ResourceInstances, hard-delete all ResourceTypes. No `IModuleClient` call from ManagementGroups to Resources.

**Rationale**:
- Decouples modules: ManagementGroups needs no knowledge of Resources' cleanup logic.
- Event-driven consistency (ADR-001 Decision) extends to lifecycle: group deletion is one semantic action producing one event, multiple subscribers handle their own cascade rules.
- `GroupDeleted` event already exists in shared abstractions; reuse closes the loop opened by ADR-001 Consequences ("Cascade behaviour on group deletion must be defined").
- Hard-delete ResourceTypes on group delete (vs. soft-delete) is acceptable because types are schema templates, not user data; instances (the actual user items) are soft-deleted for audit trail.

**Consequences**:
- Resources module must implement idempotent `GroupDeletedHandler`: if a group ID is already unknown, the handler succeeds silently (handles retries / replay).
- Cascade semantics are now split: group deletion logic lives in ManagementGroups, cleanup logic in Resources. Code review must verify both are tested.
- If future modules depend on group deletion, each must subscribe independently; no centralized cascade registry (acceptable trade-off for decoupling).

**Supersedes ADR-001 Consequence**: "Cascade behaviour on group deletion must be defined (orphaning instances vs. cleanup)" — now formally defined as soft-delete instances + hard-delete types via event subscription.

---

## Decision 3 — Unique Constraint Scoping Pattern

**Decision**: Uniqueness is always scoped to an owning aggregate, never global. Concretely:
- `ManagementGroup.Name` is unique within `(OwnerId, Name)`: two different users can own groups with the same name.
- `ResourceType.Name` is unique within `(GroupId, Name)`: different groups can have schemas with the same name.

Validation is enforced via database unique indexes and domain exception in handlers; API returns 409 Conflict on violation.

**Rationale**:
- Prevents namespace collisions while allowing semantic name reuse across owners/groups (e.g. "Camera" is a natural name for many users' schemas).
- Unique index scoping to non-deleted rows (`WHERE DeletedAt IS NULL` for soft-deleted entities) preserves audit trail while preventing genuine duplicates.
- Handler validation (throw domain exception → 409 at endpoint) is consistent with `domain-entity-design.md` and `naming-commands-queries-handlers-results.md` conventions.
- Client-side async validation (debounced availability check) gates the submit button before the user hits a 409.

**Consequences**:
- Soft-deleted rows do not block new row creation (deleted group "Acme" can be recreated after soft-delete expires from views).
- Async uniqueness checks on the frontend require a dedicated endpoint (e.g. `GET /management-groups/name-available?name=...`); this endpoint must filter by current owner.
- Unique constraints are **mandatory** for all create/update handlers; test coverage must include 409-inducing duplicate-name scenarios.

---

## Decision 4 — Deterministic Visual Identity from Entity ID

**Decision**: A group's avatar accent color is derived deterministically from `groupId` via hash modulo, not stored in the database and not user-selectable in this iteration. The hash function is stable: same `groupId` always yields the same color.

**Rationale**:
- No DB column required; color is a pure function of identity.
- User recognition: seeing the same color for the same group across views builds familiarity.
- Future migration is non-breaking: if a future iteration adds optional `colorPreference: string?` column, clients fall back to the hash when the column is null.
- Simplifies frontend routing and caching: no group-detail fetch is needed just to render an avatar in a list.

**Consequences**:
- Color palette is limited to 3 options (per design: `--color-accent-primary/secondary/tertiary`). Multiple groups will collide on color; secondary identifier (initials, text) differentiates if needed.
- Changing the hash function is a breaking change (avatars recolor for all groups); hash function must be versioned or never changed.
- Color derivation happens client-side only; no server-side avatar endpoint required.

---

## Decision 5 — Frontend Notification Service as Canonical Feedback Channel

**Decision**: A single `NotificationService` (signal-backed queue, auto-dismiss ~5s, success/error/info variants) is the canonical mechanism for transient user feedback. The `errorInterceptor` is its automatic producer: all HTTP 4xx/5xx responses trigger a notification without explicit handler calls.

**Rationale**:
- Single source of truth: prevents duplicate toasts and ad-hoc alert dialogs from diverging messaging.
- Automatic error handling: developers forget explicit error UI; interceptor ensures no silent failures.
- Stack-friendly: toasts queue and dismiss in FIFO order, respecting viewport real estate.
- Respects `prefers-reduced-motion: reduce` via CSS duration shortening; no motion is a **hard requirement** for accessibility.

**Consequences**:
- All HTTP errors surface a toast by default; success handlers must call `notificationService.success(...)` explicitly (no auto-success).
- Error message sourcing: prefer server-provided message (`error.message` or body field) over generic status-code fallback.
- `tb-toast` component must be globally rendered (e.g. in app root); no per-route instantiation.
- Custom error UI (e.g. inline field validation messages) is still valid; NotificationService is for system-level feedback, not form-level feedback.

**Relates to**: `angular-http-pattern.md` (interceptors pattern) and `angular-component-design.md` (standalone components). NotificationService unifies the precedent of error interception into a single shared service.

---

## Amendments
<!-- append-only: never edit sections above this line after initial write -->
