---
name: ef-schema-isolation
description: Every module DbContext must call modelBuilder.HasDefaultSchema(...) with the module's own lowercase snake_case schema name. Using the public schema or omitting the call causes cross-module table collisions and silent Respawn data loss in tests.
metadata:
  type: project
---

## Rule

Every `DbContext` in a module must call `modelBuilder.HasDefaultSchema(...)` in `OnModelCreating`, passing the module's schema name as a lowercase snake_case string.

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.HasDefaultSchema("{module_schema_name}");
    // ...
}
```

**Schema name derivation:** convert the module name to lowercase snake_case.

| Module name | Schema name |
|---|---|
| `Bookings` | `"bookings"` |
| `ManagementGroups` | `"management_groups"` |
| `Users` | `"users"` |
| `ResourceTypes` | `"resource_types"` |

**Forbidden values:**
- `"public"` — the PostgreSQL default; collides with every schema-less module
- Another module's schema name — causes Respawn to delete both modules' data when resetting one
- Omitting the call entirely — same result as `"public"`

## Rationale

Each module owns an isolated PostgreSQL schema. Without `HasDefaultSchema`, EF Core places all tables in `public`, which is shared across the entire database. Two consequences:

1. **Naming collisions** — two modules can accidentally create a table with the same name in `public`, breaking one or both.
2. **Respawn data pollution** — integration tests use Respawn to clean specific schemas between test runs. If a module has no schema, Respawn does not know which tables belong to it and either skips them (leaving stale data) or deletes tables it should not touch.

## Bad example

```csharp
// WRONG — no HasDefaultSchema; tables land in the public schema
internal sealed class BookingsDbContext : DbContext
{
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // No HasDefaultSchema call
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingsDbContext).Assembly);
    }
}
```

```csharp
// WRONG — explicit public schema; same problem
modelBuilder.HasDefaultSchema("public");
```

## Good example

```csharp
// CORRECT — isolated schema named after the module
internal sealed class BookingsDbContext : DbContext
{
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("bookings");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingsDbContext).Assembly);
    }
}
```

```csharp
// CORRECT — multi-word module name
internal sealed class ManagementGroupsDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("management_groups");
        // ...
    }
}
```
