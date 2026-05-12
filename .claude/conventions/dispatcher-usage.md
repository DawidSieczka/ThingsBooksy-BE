---
name: dispatcher-usage
description: Commands are dispatched via IDispatcher.SendAsync, queries via IDispatcher.QueryAsync. Direct handler instantiation, direct handler method calls, and MediatR (IMediator) are forbidden.
metadata:
  type: project
---

## Rule

All command and query dispatch in endpoint delegates and application services must go through `IDispatcher`. Two methods are available:

| Intent | Method |
|---|---|
| Execute a command (write operation, no return value) | `await dispatcher.SendAsync(command, ct)` |
| Execute a query (read operation, returns data) | `await dispatcher.QueryAsync(query, ct)` |

**Forbidden patterns:**
- Instantiating a handler directly: `new CreateBookingCommandHandler(...).HandleAsync(command, ct)` → forbidden
- Calling `HandleAsync` on an injected handler: `_handler.HandleAsync(command, ct)` → forbidden
- Importing or injecting `MediatR.IMediator` → forbidden; use `IDispatcher` instead
- Importing or injecting `AutoMapper.IMapper` → forbidden; map manually

## Rationale

`IDispatcher` is the single dispatch seam in ThingsBooksy. Routing all commands and queries through it ensures:
- **Pipeline hooks** (logging, validation, transactions, outbox) apply consistently without each caller having to wire them up.
- **Testability** — the dispatch layer can be substituted in integration tests without changing any caller.
- **Discoverability** — grepping for `SendAsync` and `QueryAsync` finds every write and read operation in the codebase.

Direct handler calls bypass the pipeline entirely. MediatR is an external dependency that duplicates what `IDispatcher` already provides and is explicitly excluded from the tech stack (see CLAUDE.md: "No MediatR").

## Bad example

```csharp
// WRONG — direct handler instantiation bypasses the dispatch pipeline
app.MapPost("/bookings", async (
    CreateBookingRequest request,
    BookingsDbContext db,
    HttpContext context) =>
{
    var handler = new CreateBookingCommandHandler(db); // bypasses pipeline
    await handler.HandleAsync(new CreateBookingCommand(...), default);
    return Results.Created();
});
```

```csharp
// WRONG — MediatR injection
app.MapPost("/bookings", async (
    CreateBookingRequest request,
    IMediator mediator,          // forbidden
    HttpContext context) =>
{
    await mediator.Send(new CreateBookingCommand(...));
    return Results.Created();
});
```

## Good example

```csharp
// CORRECT — all dispatch via IDispatcher
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
```
