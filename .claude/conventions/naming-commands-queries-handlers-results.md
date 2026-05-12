---
name: naming-commands-queries-handlers-results
description: Naming rules for command classes, query classes, their handlers, result types, and folder structure for DTOs and nested result types
metadata:
  type: feedback
---

## Description

All command classes, query classes, their handlers, and their result types must
follow these naming rules. The rules are mechanical — given the feature and
module, the correct name for every type is deterministic.

### 1. PascalCase everywhere
Class names and file names always use PascalCase. camelCase, snake_case, or any
other casing is forbidden for both type names and the `.cs` files that contain
them.

### 2. Full unambiguous names
Class names must include enough context — module scope, aggregate scope, or both
— that no two classes in the solution could be confused. Short names that omit
that context are forbidden.

### 3. Suffix rules

| Type | Suffix | Example |
|---|---|---|
| Command | `Command` | `CreateManagementGroupCommand` |
| Command handler | `CommandHandler` | `CreateManagementGroupCommandHandler` |
| Query | `Query` | `GetManagementGroupQuery` |
| Query handler | `QueryHandler` | `GetManagementGroupQueryHandler` |

### 4. One class per file, file name = class name exactly
A file named `CreateManagementGroupCommand.cs` contains exactly one type:
`CreateManagementGroupCommand`. No bundling of related types in one file.

### 5. Result classes
A handler that returns data must return a dedicated result record. The name is
derived mechanically from the handler name: strip the `Handler` suffix and
nothing else.

- `CreateManagementGroupCommandHandler` → returns `CreateManagementGroupCommandResult`
- `GetManagementGroupQueryHandler` → returns `GetManagementGroupQueryResult`

A handler that performs an action and returns no data returns `Task` — no Result
class is created.

### 6. Result class placement
The Result record lives in the same feature folder as its handler. It is not
placed in a shared folder or a central `Results/` directory.

### 7. Models/ subfolder
Internal grouping DTOs — types that are only used by a single handler and are
not exposed outside it — live in a `Models/` subfolder inside the feature
folder.

### 8. Models/Results/ subfolder
Nested types used inside a Result record (i.e., the record contains a collection
or property of another structured type) live in `Models/Results/`. These types
are named `<Concept>Result` (e.g., `ManagementGroupMemberResult`).

---

## Rationale

Inconsistent naming forces every developer to guess the correct name of a type
before they can find or reference it. In a modular monolith where many modules
define commands and queries for similar aggregates (e.g., both a Users module and
a ManagementGroups module may have a `GetMembers` concept), short names cause
collisions, ambiguity, and incorrect cross-module imports. Mechanical suffixes
and full-context names eliminate all guessing: given a feature description, the
file name and type name are derivable without reading any code.

The one-class-per-file rule ensures that a file search for `GetManagementGroupQuery`
returns exactly one result and that file contains exactly that type.

Deriving Result names from handler names mechanically means the pairing is
unambiguous — a reader seeing a handler knows immediately what record to look for.

---

## Bad Example

```csharp
// WRONG: casing, missing context, wrong suffixes, multiple types per file,
// result type not derived from handler name.

// File: createGroup.cs  (camelCase, multiple types in one file)

internal record Create(Guid OwnerId, string Name) : ICommand;   // no suffix, no module context

internal class CreateHandler : ICommandHandler<Create>           // no suffix, ambiguous name
{
    public async Task<GroupDto> HandleAsync(Create command, CancellationToken ct)  // arbitrary name, not derived
    {
        // ...
        return new GroupDto(command.OwnerId, command.Name);
    }
}

internal record GroupDto(Guid Id, string Name);   // not in Models/, name gives no context
```

```csharp
// WRONG: short query name collides with identically-named type in another module

// In ManagementGroups module:
internal record GetMembersQuery(Guid GroupId) : IQuery<MembersResult>;

// In Users module:
internal record GetMembersQuery(Guid UserId) : IQuery<MembersResult>;  // collision
```

## Good Example

```csharp
// CORRECT layout for a command that creates a management group and returns data.

// File: CreateManagementGroupCommand.cs
internal record CreateManagementGroupCommand(Guid GroupId, Guid OwnerId, string Name)
    : ICommand;

// File: CreateManagementGroupCommandHandler.cs
internal sealed class CreateManagementGroupCommandHandler
    : ICommandHandler<CreateManagementGroupCommand, CreateManagementGroupCommandResult>
{
    public async Task<CreateManagementGroupCommandResult> HandleAsync(
        CreateManagementGroupCommand command, CancellationToken ct)
    {
        // ...
        return new CreateManagementGroupCommandResult(command.GroupId);
    }
}

// File: CreateManagementGroupCommandResult.cs  (same feature folder as handler)
internal record CreateManagementGroupCommandResult(Guid GroupId);
```

```csharp
// CORRECT layout for a query that returns a group with its members.

// File: GetManagementGroupQueryHandler.cs
internal sealed class GetManagementGroupQueryHandler
    : IQueryHandler<GetManagementGroupQuery, GetManagementGroupQueryResult>
{
    public async Task<GetManagementGroupQueryResult> HandleAsync(
        GetManagementGroupQuery query, CancellationToken ct)
    {
        // ...
        return new GetManagementGroupQueryResult(
            group.Id, group.Name,
            members.Select(m => new ManagementGroupMemberResult(m.UserId, m.Role)).ToList());
    }
}

// File: GetManagementGroupQueryResult.cs  (same feature folder)
internal record GetManagementGroupQueryResult(
    Guid Id,
    string Name,
    IReadOnlyList<ManagementGroupMemberResult> Members);

// File: Models/Results/ManagementGroupMemberResult.cs
internal record ManagementGroupMemberResult(Guid UserId, string Role);
```

```csharp
// CORRECT: void handler — no Result class

// File: DeleteManagementGroupCommandHandler.cs
internal sealed class DeleteManagementGroupCommandHandler
    : ICommandHandler<DeleteManagementGroupCommand>
{
    public async Task HandleAsync(DeleteManagementGroupCommand command, CancellationToken ct)
    {
        // ...
    }
}
// No DeleteManagementGroupCommandResult.cs — handler returns Task, not data.
```
