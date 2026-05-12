---
name: internals-visible-to
description: Every module's .Core project must declare four InternalsVisibleTo attributes — for .Api, .Migrations, .IntegrationTests, and DynamicProxyGenAssembly2 — or the solution will not compile / mocking will fail at runtime.
metadata:
  type: project
---

## Rule

Every `.Core` project in a module must declare the following four `[assembly: InternalsVisibleTo(...)]` attributes. Place them in `{Module}.Core/Extensions.cs` (alongside the `Add{ModuleName}Core` registration method).

```csharp
[assembly: InternalsVisibleTo("ThingsBooksy.Modules.{ModuleName}.Api")]
[assembly: InternalsVisibleTo("ThingsBooksy.Modules.{ModuleName}.Migrations")]
[assembly: InternalsVisibleTo("ThingsBooksy.Modules.{ModuleName}.IntegrationTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
```

All four are required. Missing any one of them causes:
- `.Api` missing → compilation error: endpoint delegates cannot access `internal` command/query types
- `.Migrations` missing → compilation error: the migrations project cannot access the `internal` `DbContext`
- `.IntegrationTests` missing → compilation error: test factories cannot access `internal` entities or `DbContext`
- `DynamicProxyGenAssembly2` missing → runtime failure when a mock framework (e.g. Moq, NSubstitute) tries to subclass an `internal` type

## Rationale

Module internals are marked `internal` to enforce module boundary isolation — no other module may import them. However, the same module's own satellite projects (`.Api`, `.Migrations`, `.IntegrationTests`) and mock-generation assemblies legitimately need access. `InternalsVisibleTo` grants that access at compile time without relaxing the `internal` modifier for the rest of the solution.

## Bad example

```csharp
// Extensions.cs — missing InternalsVisibleTo declarations
internal static class Extensions
{
    public static IServiceCollection AddBookingsCore(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .AddDataProviders([typeof(Extensions).Assembly])
            .AddPostgres<BookingsDbContext>(configuration, "ThingsBooksy.Modules.Bookings.Migrations");
}
// Result: BookingsModule.cs in .Api cannot see BookingsDbContext or any internal command type.
```

## Good example

```csharp
// Extensions.cs — all four attributes present
[assembly: InternalsVisibleTo("ThingsBooksy.Modules.Bookings.Api")]
[assembly: InternalsVisibleTo("ThingsBooksy.Modules.Bookings.Migrations")]
[assembly: InternalsVisibleTo("ThingsBooksy.Modules.Bookings.IntegrationTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace ThingsBooksy.Modules.Bookings.Core;

internal static class Extensions
{
    public static IServiceCollection AddBookingsCore(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .AddDataProviders([typeof(Extensions).Assembly])
            .AddPostgres<BookingsDbContext>(configuration, "ThingsBooksy.Modules.Bookings.Migrations")
            .AddOutbox<BookingsDbContext>(configuration)
            .AddUnitOfWork<BookingsUnitOfWork>();
}
```
