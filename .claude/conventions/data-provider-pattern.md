---
name: data-provider-pattern
description: Handlers (command and query) must not inject DbContext directly — use a dedicated IDataProvider interface per handler, co-located in Features/{Feature}/DataProviders/. The AddDataProviders helper in Shared.Infrastructure scans and registers all providers; each module calls it from its own Extensions.cs.
---

## Rule

Handlers (both command and query) must **never** inject `DbContext` (or any derived `DbContext`) directly. Instead, each handler depends on a dedicated interface that is named after the handler, suffixed with `DataProvider`, and marked with `IDataProvider`.

## Definitions

| Term | Location | Responsibility |
|---|---|---|
| `IDataProvider` | `Shared.Abstractions/DataProviders/IDataProvider.cs` | Marker interface — no members |
| `IXxxDataProvider` | `{Module}.Core/Features/{Feature}/DataProviders/` | One interface per handler; declares only the queries/commands that handler needs |
| `XxxDataProvider` | `{Module}.Core/Features/{Feature}/DataProviders/` | Concrete EF Core implementation; injects `DbContext` |
| `AddDataProviders` | `Shared.Infrastructure/DataProviders/Extensions.cs` | Assembly-scanning helper; registers every `IDataProvider` as `Scoped` |

## Naming

The interface name is derived mechanically from the handler name: strip `Handler`, append `DataProvider`.

- `GetManagementGroupQueryHandler` → `IGetManagementGroupQueryDataProvider`
- `GetResourceTypesQueryHandler` → `IGetResourceTypesQueryDataProvider`
- `CreateResourceTypeCommandHandler` → `ICreateResourceTypeCommandDataProvider`
- `DeleteResourceInstanceCommandHandler` → `IDeleteResourceInstanceCommandDataProvider`

The concrete class carries the same name without the leading `I`:
`GetManagementGroupQueryDataProvider`.

Both files live in `{Module}.Core/Features/{Feature}/DataProviders/`, co-located with the handler.

## Structure inside a module

```
{Module}.Core/
  Features/
    GetFoo/
      DataProviders/
        IGetFooQueryDataProvider.cs          ← interface
        GetFooQueryDataProvider.cs           ← EF Core implementation
      GetFooQuery.cs
      GetFooHandler.cs
    CreateFoo/
      DataProviders/
        ICreateFooCommandDataProvider.cs
        CreateFooCommandDataProvider.cs
      CreateFooCommand.cs
      CreateFooCommandHandler.cs
  DAL/
    {Module}DbContext.cs
```

## Registration

`AddDataProviders` lives in `Shared.Infrastructure` and scans assemblies for all types that implement `IDataProvider`, registering each as `Scoped` under its declared interface(s).

```csharp
// Shared.Infrastructure/DataProviders/Extensions.cs
namespace ThingsBooksy.Shared.Infrastructure.DataProviders;

public static class Extensions
{
    public static IServiceCollection AddDataProviders(
        this IServiceCollection services,
        IEnumerable<Assembly> assemblies)
    {
        services.Scan(s => s.FromAssemblies(assemblies)
            .AddClasses(c => c.AssignableTo<IDataProvider>())
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
```

Each module calls `AddDataProviders` from its own `{Module}.Core/Extensions.cs`, passing its own assembly:

```csharp
// {Module}.Core/Extensions.cs
internal static class Extensions
{
    public static IServiceCollection AddFooCore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services
            // ... commands and other query handlers ...
            .AddDataProviders([typeof(Extensions).Assembly])
            .AddPostgres<FooDbContext>(configuration, "ThingsBooksy.Modules.Foo.Migrations")
            .AddOutbox<FooDbContext>(configuration)
            .AddUnitOfWork<FooUnitOfWork>();
    }
}
```

Do **not** call `AddDataProviders` in the bootstrapper or in `AddModularInfrastructure` — each module owns its own providers.

## Async eliding

Data provider methods must **not** use `async`/`await` when the body is a single awaitable expression. Return the `Task` directly.

```csharp
// CORRECT — no unnecessary state machine
public Task<ResourceTypeDto?> GetAsync(Guid id, CancellationToken ct)
    => _dbContext.ResourceTypes
        .Where(x => x.Id == id)
        .Select(x => new ResourceTypeDto(...))
        .FirstOrDefaultAsync(ct);

// WRONG — spurious async/await
public async Task<ResourceTypeDto?> GetAsync(Guid id, CancellationToken ct)
    => await _dbContext.ResourceTypes ...;
```

If the method contains branching, guards, or multiple awaits, use `async`/`await` normally.

## One DataProvider per handler

Each handler (command or query) has exactly one `IXxxDataProvider`. Do not create a shared data provider that multiple handlers depend on. A handler that needs data from two different tables may declare two methods on its own provider interface.

## Rationale

Injecting `DbContext` directly into handlers couples database access to business logic in a way that is hard to test and hard to isolate. A dedicated `IDataProvider` interface per handler gives each handler a contract it owns, makes the dependency explicit, and allows the test to substitute an in-memory implementation without spinning up a database. Centralising registration in `AddDataProviders` (one call per module) eliminates the error-prone manual `.AddScoped<IFoo, Foo>()` line that must be added whenever a new provider is created.

## Bad example

```csharp
// WRONG — handler injects DbContext directly (applies to both command and query handlers)
internal sealed class GetResourceTypeHandler
    : IQueryHandler<GetResourceTypeQuery, ResourceTypeDto?>
{
    private readonly ResourcesDbContext _dbContext; // direct dependency on EF type

    public GetResourceTypeHandler(ResourcesDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<ResourceTypeDto?> HandleAsync(
        GetResourceTypeQuery query, CancellationToken ct = default)
    {
        return await _dbContext.ResourceTypes // <-- EF query inside the handler
            .Include(x => x.PropertyDefinitions)
            .FirstOrDefaultAsync(x => x.Id == query.TypeId, ct);
    }
}
```

## Good example

```csharp
// Shared.Abstractions/DataProviders/IDataProvider.cs
namespace ThingsBooksy.Shared.Abstractions.DataProviders;

// Marker — no members
public interface IDataProvider { }
```

```csharp
// Resources.Core/Features/GetResourceType/DataProviders/IGetResourceTypeQueryDataProvider.cs
namespace ThingsBooksy.Modules.Resources.Core.Features.GetResourceType.DataProviders;

internal interface IGetResourceTypeQueryDataProvider : IDataProvider
{
    Task<ResourceTypeDto?> GetByIdAsync(
        Guid typeId, Guid requesterId, CancellationToken ct);
}
```

```csharp
// Resources.Core/Features/GetResourceType/DataProviders/GetResourceTypeQueryDataProvider.cs
namespace ThingsBooksy.Modules.Resources.Core.Features.GetResourceType.DataProviders;

internal sealed class GetResourceTypeQueryDataProvider
    : IGetResourceTypeQueryDataProvider
{
    private readonly ResourcesDbContext _dbContext;

    public GetResourceTypeQueryDataProvider(ResourcesDbContext dbContext)
        => _dbContext = dbContext;

    // Single awaitable — return Task directly, no async/await
    public Task<ResourceTypeDto?> GetByIdAsync(
        Guid typeId, Guid requesterId, CancellationToken ct)
        => _dbContext.ResourceTypes
            .Where(x => x.Id == typeId)
            .Select(x => new ResourceTypeDto(
                x.Id,
                x.GroupId,
                x.Name,
                x.Description,
                x.CreatedAt,
                x.PropertyDefinitions
                    .Select(d => new PropertyDefinitionDto(
                        d.Id, d.Name, d.DataType.ToString(), d.IsRequired))
                    .ToList()))
            .FirstOrDefaultAsync(ct);
}
```

```csharp
// Resources.Core/Features/GetResourceType/GetResourceTypeHandler.cs
namespace ThingsBooksy.Modules.Resources.Core.Features.GetResourceType;

internal sealed class GetResourceTypeHandler
    : IQueryHandler<GetResourceTypeQuery, ResourceTypeDto?>
{
    private readonly IGetResourceTypeQueryDataProvider _provider;

    public GetResourceTypeHandler(IGetResourceTypeQueryDataProvider provider)
        => _provider = provider;

    public Task<ResourceTypeDto?> HandleAsync(
        GetResourceTypeQuery query, CancellationToken ct = default)
        => _provider.GetByIdAsync(query.TypeId, query.RequesterId, ct);
}
```

```csharp
// Resources.Core/Extensions.cs  — call AddDataProviders once, pass own assembly
internal static class Extensions
{
    public static IServiceCollection AddResourcesCore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services
            .AddScoped<ICommandHandler<CreateResourceTypeCommand>, CreateResourceTypeHandler>()
            // other command registrations ...
            .AddDataProviders([typeof(Extensions).Assembly])   // scans DataProviders/ automatically
            .AddPostgres<ResourcesDbContext>(configuration, "ThingsBooksy.Modules.Resources.Migrations")
            .AddOutbox<ResourcesDbContext>(configuration)
            .AddUnitOfWork<ResourcesUnitOfWork>();
    }
}
```
