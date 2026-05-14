# Shared event contracts — feature 010

This feature reuses one existing event from `backend/src/Shared/ThingsBooksy.Shared.Abstractions/Events/ManagementGroups/`. No new shared contracts are introduced.

## `GroupDeleted` (REUSED — no change)

Defined in `backend/src/Shared/ThingsBooksy.Shared.Abstractions/Events/ManagementGroups/GroupDeleted.cs`:

```csharp
public sealed record GroupDeleted(Guid GroupId) : IEvent;
```

**Published by**: `DeleteManagementGroupHandler` (ManagementGroups module) after the entity is marked `DeletedAt = now()` and SaveChanges committed. Publish via `IMessageBroker.PublishAsync(...)` inside the same Unit-of-Work as the entity save (outbox pattern handled by infrastructure).

**Subscribed by (new in this feature)**:
- `Resources` module — `GroupDeletedHandler` performs the cascade documented in `research.md §2` and `data-model.md §State transitions`. Idempotent.

**Subscribed by (existing)**:
- `Resources` module — `GroupDeletedHandler` already removes the `GroupReadModel` row. *Plan note*: if the existing handler is named `GroupDeletedHandler` and lives in `Features/.../EventHandlers/`, the new cascade logic is added to the same handler (extends it). Otherwise a second handler `GroupCascadeHandler` is added. The `module-writer` agent decides at implementation time based on existing code structure; both approaches preserve the constitution.

**Idempotency contract**: Re-delivery of the same `GroupDeleted` event by the message broker (or by Outbox retry) MUST be a no-op — no exceptions thrown, no rows changed.

**Ordering contract**: `GroupDeleted` is published after the group's `DeletedAt` is committed. The cascade subscriber MAY assume the group row is already soft-deleted on the ManagementGroups side (not required, but useful for diagnostics).

---

## Events explicitly NOT introduced

The following were considered and rejected (research.md §2):
- `ResourcesCascadeCompleted` (post-cleanup) — not needed; no subscriber requires the signal in this iteration.
- `GroupNameChanged` — not needed; group name changes are not consumed by Resources for any read-model.
- `ResourceTypeCreated` / `ResourceTypeDeleted` — internal to Resources, not cross-module.
- `ResourceInstanceDeleted` — internal to Resources, not cross-module.

If any of these are needed by a future feature, define them then; do not pre-emptively introduce them now.
