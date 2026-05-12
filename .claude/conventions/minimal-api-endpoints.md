---
name: minimal-api-endpoints
description: All HTTP endpoints use Minimal API only (no MVC controllers). Endpoints are registered in the Expose() method of the module class. Every route must use the /{module-name}/... prefix in kebab-case. AddEndpointsApiExplorer() must be called in Register().
metadata:
  type: project
---

## Rule

### 1. Minimal API only — no MVC controllers

No class in any module may inherit from `ControllerBase` or carry an `[ApiController]` attribute. All HTTP endpoints are registered using the Minimal API `Map*` methods.

### 2. Endpoints in `Expose()`

Endpoint registrations belong exclusively in the `Expose(IEndpointRouteBuilder app)` method of `{ModuleName}Module.cs`. Registering endpoints anywhere else (e.g., inside `Register()`, in extension methods called from `Program.cs`) is forbidden.

### 3. Route prefix — `/{module-name}/...`

Every route must start with the module's kebab-case name as the first path segment.

| Module | Prefix |
|---|---|
| `Bookings` | `/bookings/...` |
| `ManagementGroups` | `/management-groups/...` |
| `Users` | `/users/...` |
| `ResourceTypes` | `/resource-types/...` |

Nested resources follow the same prefix: `/management-groups/{id}/members`, not `/members/{id}`.

### 4. `AddEndpointsApiExplorer()` in `Register()`

The `Register(IServiceCollection services)` method must call `services.AddEndpointsApiExplorer()`. Without it, Swagger does not discover Minimal API endpoints and the module is invisible in the API documentation.

### Relationship to command construction

How a request body is parsed inside an endpoint — specifically, the requirement to use a request DTO instead of binding a command directly — is covered by the [[command-construction-in-endpoints]] convention. This convention covers structure and routing; that one covers security and binding.

## Rationale

Minimal API keeps the framework surface small and avoids the MVC pipeline overhead. A consistent route prefix per module makes the API surface self-documenting — any route reveals which module owns it. Centralising endpoint registration in `Expose()` ensures that all routes for a module are in one place and that module startup is symmetric across all modules.

## Bad example

```csharp
// WRONG — MVC controller
[ApiController]
[Route("bookings")]
public class BookingsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request) { ... }
}
```

```csharp
// WRONG — endpoints registered outside Expose()
public class BookingsModule : IModule
{
    public void Register(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        // endpoint registered here instead of Expose() — WRONG
        services.AddSingleton<IEndpointFilter, ...>();
    }

    public void Expose(IEndpointRouteBuilder app)
    {
        // empty — all registrations leaked into Register()
    }
}
```

```csharp
// WRONG — missing module prefix
app.MapPost("/create-booking", ...);     // no module prefix
app.MapGet("/booking/{id}", ...);        // wrong prefix
```

## Good example

```csharp
// CORRECT — Minimal API, Expose() method, correct prefix, AddEndpointsApiExplorer in Register()
public class BookingsModule : IModule
{
    public void Register(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();   // required for Swagger
        services.AddBookingsCore(/* config */);
    }

    public void Expose(IEndpointRouteBuilder app)
    {
        app.MapPost("/bookings", async (
            CreateBookingRequest request,
            IDispatcher dispatcher,
            HttpContext context) =>
        {
            var command = new CreateBookingCommand(
                BookingId: Guid.CreateVersion7(),
                OwnerId: GetUserId(context),
                Name: request.Name);
            await dispatcher.SendAsync(command);
            return Results.Created($"/bookings/{command.BookingId}", new { id = command.BookingId });
        }).RequireAuthorization();

        app.MapGet("/bookings/{id:guid}", async (
            Guid id,
            IDispatcher dispatcher,
            HttpContext context) =>
        {
            var result = await dispatcher.QueryAsync(new GetBookingQuery(id, GetUserId(context)));
            return result is null ? Results.NotFound() : Results.Ok(result);
        }).RequireAuthorization();
    }
}
```
