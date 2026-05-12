---
name: integration-test-naming
description: Integration test method names follow the pattern {Action}{Entity}_{Condition}_{Result}. The last segment is an HTTP status code when status is the primary assertion, or a behavioral description when the assertion covers side effects or domain behavior.
metadata:
  type: feedback
---

## Rule

Every `[Fact]` method in an integration test class must follow this naming pattern:

```
{Action}{Entity}_{Condition}_{Result}
```

| Segment | Describes | Examples |
|---|---|---|
| `{Action}{Entity}` | What operation is being tested | `CreateGroup`, `DeleteMember`, `GetBookings`, `RestoreResource` |
| `{Condition}` | The specific scenario or input state | `WithValidData`, `AsNonOwner`, `WhenSoftDeleted`, `WithInvalidEmailFormat` |
| `{Result}` | The expected outcome | `Returns201AndPersistsInDb`, `Returns403`, `Returns404`, `DoesNotReturnDeletedGroups` |

### Choosing the `{Result}` segment

Use an **HTTP status code** when the status code is the primary and sufficient assertion:
```
DeleteGroup_AsNonOwner_Returns403
GetGroup_WhenNotFound_Returns404
AddMember_WithInvalidEmailFormat_Returns400
```

Use a **behavioral description** when the assertion covers more than just the status — a side effect, a DB state, or a domain-level invariant:
```
CreateGroup_WithValidData_Returns201AndPersistsInDb
GetGroups_DoesNotReturnDeletedGroups
DeleteGroup_SetsDeletedAt_InDatabase
```

### PascalCase throughout

All three segments use PascalCase. No underscores within a segment, no camelCase, no snake_case.

## Rationale

The `{Action}{Entity}_{Condition}_{Result}` pattern makes test intent readable without opening the method body. The split on `_` gives a consistent structure: what, under what condition, with what outcome. Using status codes in the result segment avoids verbose invented names for simple status assertions; using behavioral descriptions for complex assertions avoids misleading single-status names that hide the real assertion (e.g., a test named `Returns201` that also asserts DB state would be underselling its coverage).

## Bad example

```csharp
// WRONG — vague, no condition, no result
[Fact]
public async Task CreateBooking() { ... }

// WRONG — verb-first instead of Action+Entity, missing segments
[Fact]
public async Task ShouldReturn200WhenCreated() { ... }

// WRONG — camelCase segments
[Fact]
public async Task createGroup_withValidData_returns201() { ... }

// WRONG — status code where behavioral description is needed (hides DB assertion)
[Fact]
public async Task CreateGroup_WithValidData_Returns201()
{
    // actually also asserts DB state — name is misleading
}
```

## Good example

```csharp
// CORRECT — status as result (status is the full assertion)
[Fact]
public async Task DeleteGroup_AsNonOwner_Returns403() { ... }

// CORRECT — behavioral result (status + side effect)
[Fact]
public async Task CreateGroup_WithValidData_Returns201AndPersistsInDb() { ... }

// CORRECT — behavioral result (domain invariant, no specific status)
[Fact]
public async Task GetGroups_DoesNotReturnDeletedGroups() { ... }

// CORRECT — condition is a state, result is a status
[Fact]
public async Task GetGroup_WhenSoftDeleted_Returns404() { ... }

// CORRECT — DB-level assertion named explicitly
[Fact]
public async Task DeleteGroup_ByOwner_SetsDeletedAtInDb() { ... }
```
