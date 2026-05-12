---
name: domain-entity-design
description: Structure, property access modifiers, factory methods, mutation methods, computed properties, nav props, parameter limits, and read-model conventions for all domain entities and read-models in ThingsBooksy.
metadata:
  type: project
---

## Rule

Every domain entity and read-model must follow this layout, in this exact member order:

1. **Properties** — all `private set`, declared before constructors
2. **Computed properties** — expression-bodied, derived from stored state (optional section)
3. **Private parameterless constructor** — EF Core materialization hook
4. **Factory method** — `public static TEntity Create(...)` for entities; `internal static TReadModel Upsert(TEvent @event)` for read-models
5. **Mutation methods** — `Update`, `Delete`, `Restore`, etc. (entities only)
6. **Navigation properties** — `private set`; initialised inline; cleaned up on next touch

---

## Property access modifiers

All properties on both domain entities and read-models carry `private set`. No `public set`. No `init`. No `{ get; }` auto-property without a setter.

EF Core materializes objects by writing directly to private backing fields via reflection or compiled expression trees — it bypasses access modifiers entirely. `private set` therefore works with EF Core, and it is required for encapsulation.

```csharp
// CORRECT
public Guid Id { get; private set; }
public string Name { get; private set; } = null!;
public DateTime? DeletedAt { get; private set; }

// WRONG — exposes mutation surface
public Guid Id { get; set; }
public string Name { get; set; } = null!;

// WRONG — init-only is not enough; it prevents Update() from mutating
public string Name { get; init; } = null!;
```

---

## Private parameterless constructor

Every entity and read-model must have `private TEntity() { }`. EF Core calls this constructor first, then fills properties. Without it the runtime throws at materialization time.

```csharp
private ManagementGroup() { }
```

---

## Factory methods — entities: `Create`

Entities are created only through a `public static TEntity Create(...)` method. Direct construction (`new ManagementGroup { ... }` from outside the class) is forbidden.

**Parameter convention** (shared with the [command-construction-in-endpoints](command-construction-in-endpoints.md) convention):

- Pass a command object as the first parameter when the entity maps directly to a command.
- Resolved external data (foreign keys, current timestamp, etc.) is passed as additional parameters after the command object.
- Maximum **4 parameters total** across all parameters.
- If the parameter count would exceed 4, introduce a dedicated parameter object.

`Id` is always generated inside `Create` using `Guid.CreateVersion7()`. Foreign-key references such as `OwnerId` or `GroupId` are accepted from outside.

```csharp
// Single command object — simplest case
public static Booking Create(CreateBookingCommand command)
    => new() { Id = Guid.CreateVersion7(), Name = command.Name, OwnerId = command.OwnerId };

// Command + resolved external data (still ≤ 4 parameters)
public static Booking Create(CreateBookingCommand command, Guid resolvedGroupId, DateTime now)
    => new() { Id = Guid.CreateVersion7(), Name = command.Name, GroupId = resolvedGroupId, CreatedAt = now };
```

---

## Factory methods — read-models: `Upsert`

Read-models are created and updated through a single `internal static TReadModel Upsert(TEvent @event)` method. The name signals that the method covers both creation and update semantics when the consumer receives an event (there is no prior state to distinguish the two cases).

```csharp
internal static UserReadModel Upsert(UserSignedUp @event)
    => new() { Id = @event.UserId, Email = @event.Email.ToLowerInvariant() };
```

The event handler calls `Upsert`, then performs the EF Core upsert pattern:

```csharp
internal sealed class UserSignedUpHandler : IEventHandler<UserSignedUp>
{
    private readonly SomeDbContext _dbContext;

    public UserSignedUpHandler(SomeDbContext dbContext) => _dbContext = dbContext;

    public async Task HandleAsync(UserSignedUp @event, CancellationToken ct = default)
    {
        var model = UserReadModel.Upsert(@event);
        await _dbContext.UserReadModels
            .Upsert(model)
            .On(x => x.Id)
            .RunAsync(ct);
    }
}
```

---

## Mutation methods — entities

State changes after construction happen only through explicit domain methods. Direct property assignment from outside the class is forbidden.

Naming — use the past-participle action:

| Method | When |
|---|---|
| `Update(...)` | Replaces mutable fields; always sets `UpdatedAt` |
| `Delete(DateTime now)` | Sets `DeletedAt` and `UpdatedAt` |
| `Restore(DateTime now)` | Clears `DeletedAt`, sets `UpdatedAt` |
| Other verb-based names | For domain-specific state transitions |

```csharp
public void Update(UpdateManagementGroupCommand command, DateTime now)
{
    Name = command.Name;
    Description = command.Description;
    UpdatedAt = now;
}

public void Delete(DateTime now)
{
    DeletedAt = now;
    UpdatedAt = now;
}

public void Restore(DateTime now)
{
    DeletedAt = null;
    UpdatedAt = now;
}
```

Read-models do **not** have mutation methods. They are replaced entirely via `Upsert`.

---

## Computed properties

Computed properties are placed between the stored properties and the private constructor. They derive their value from stored state and must be expression-bodied.

Use `HasQueryFilter` with the actual stored column expression (`x.DeletedAt == null`), never with the computed property (`!x.IsDeleted`), because EF Core cannot translate computed properties to SQL.

```csharp
// Stored properties
public DateTime? DeletedAt { get; private set; }

// Computed property — between properties and constructor
public bool IsDeleted => DeletedAt.HasValue;

// Private constructor
private ManagementGroup() { }
```

---

## Navigation properties

Navigation properties carry `private set` and are initialised inline. They are declared last, after mutation methods.

```csharp
public ICollection<GroupMember> Members { get; private set; } = new List<GroupMember>();
public ManagementGroup Group { get; private set; } = null!;
```

Existing nav props that violate this rule (e.g., `public set`) are corrected the next time that file is touched — do not leave a file with mixed conventions.

---

## Complete examples

### Domain entity

```csharp
namespace ThingsBooksy.Modules.ManagementGroups.Core.Domain;

internal class ManagementGroup
{
    // 1. Properties
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid OwnerId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    // 2. Computed properties
    public bool IsDeleted => DeletedAt.HasValue;

    // 3. Private constructor
    private ManagementGroup() { }

    // 4. Factory method
    public static ManagementGroup Create(CreateManagementGroupCommand command, DateTime now)
        => new()
        {
            Id = Guid.CreateVersion7(),
            Name = command.Name,
            Description = command.Description,
            OwnerId = command.OwnerId,
            CreatedAt = now,
            UpdatedAt = now
        };

    // 5. Mutation methods
    public void Update(UpdateManagementGroupCommand command, DateTime now)
    {
        Name = command.Name;
        Description = command.Description;
        UpdatedAt = now;
    }

    public void Delete(DateTime now)
    {
        DeletedAt = now;
        UpdatedAt = now;
    }

    public void Restore(DateTime now)
    {
        DeletedAt = null;
        UpdatedAt = now;
    }

    // 6. Navigation properties
    public ICollection<GroupMember> Members { get; private set; } = new List<GroupMember>();
}
```

### Read-model

```csharp
namespace ThingsBooksy.Modules.ManagementGroups.Core.ReadModels;

internal class UserReadModel
{
    // 1. Properties — private set, same as entities
    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;

    // 3. Private constructor — EF Core still needs this for DB reads
    private UserReadModel() { }

    // 4. Factory method — Upsert signals create-or-replace semantics
    internal static UserReadModel Upsert(UserSignedUp @event)
        => new() { Id = @event.UserId, Email = @event.Email.ToLowerInvariant() };
}
```

---

## Bad example

```csharp
// WRONG — violates multiple rules

internal class ManagementGroup
{
    public Guid Id { get; set; }          // public set — exposes mutation
    public string Name { get; set; } = null!;

    // No private constructor — EF Core may fail at materialization

    // Factory uses raw primitives beyond 4 params
    public static ManagementGroup Create(
        Guid id, string name, string? description, Guid ownerId, DateTime now, bool active)
        => new() { ... };

    // Mutation from outside bypassing domain method
    // (only possible because of public set)
}

internal class UserReadModel
{
    public Guid Id { get; set; }          // public set
    public string Email { get; set; } = null!;

    // Uses Create instead of Upsert — hides that it covers update too
    internal static UserReadModel Create(UserSignedUp @event)
        => new() { Id = @event.UserId, Email = @event.Email };
}
```

---

## Rationale

`private set` enforces that domain state transitions happen only through the explicit mutation methods defined on the entity — any code that bypasses them produces a compile error. EF Core's ability to hydrate `private set` properties via reflection means this constraint costs nothing at runtime.

`Upsert` as the read-model factory name makes it immediately clear that a single code path handles both first-write and overwrite scenarios driven by events. Calling it `Create` implies it can only be called once per entity, which is misleading for event-driven projections.

The max-4-parameter limit keeps factory and mutation signatures readable without requiring a reader to count through a long argument list to understand what data is required.

Computed properties between stored properties and the constructor keep logically derived state close to the stored state it derives from, while the private constructor placement acts as a clear separator between data declarations and behavior.
