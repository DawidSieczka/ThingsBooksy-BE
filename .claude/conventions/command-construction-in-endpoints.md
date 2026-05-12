---
name: command-construction-in-endpoints
description: Commands must never be bound directly from the HTTP body — use a request DTO for client fields and construct the command explicitly in the endpoint lambda.
---

## Rule

Commands must **never** be bound directly from the HTTP request body. Each endpoint that receives a body must declare a dedicated request DTO record (in the `.Api` project) containing only the fields the client is permitted to supply. The endpoint handler constructs the command explicitly, injecting server-sourced fields (route parameters, JWT-derived IDs, server-generated IDs) at the call site.

Request DTO records live in `{Module}.Api/Requests/` with namespace `{Module}.Api.Requests` — one file per record, named after the feature (e.g., `Requests/CreateGroupRequest.cs`). Do not place them inline in the endpoint file or alongside endpoint registration classes.

Body-less endpoints (DELETE, restore actions with no body) may construct the command inline without a request DTO.

## Rationale

ASP.NET Core's JSON model binder will populate any settable or `init`-only property on a type it deserializes — including properties meant to be set by the server (e.g., `OwnerId` from JWT, IDs from route, server-generated GUIDs). If a command is bound directly from the body, a client can supply those fields in the JSON payload and silently override server-sourced values. Using a separate request DTO that contains only client-permitted fields makes this impossible at the type level — there is no property on the DTO for the client to target. No attribute, no discipline, no runtime check required.

## Bad example

```csharp
// WRONG — command is bound directly from the HTTP body.
// A client can send { "name": "x", "ownerId": "attacker-guid" } and override
// the server-sourced OwnerId.

public record CreateGroupCommand(string Name, string? Description, Guid OwnerId) : ICommand;

endpoints.MapPost("/management-groups", async (
    CreateGroupCommand command,   // <-- JSON binder touches the command
    IDispatcher dispatcher,
    HttpContext context) =>
{
    command = command with { OwnerId = GetUserId(context) }; // too late; easy to forget
    await dispatcher.SendAsync(command);
    return Results.Created();
}).RequireAuthorization();
```

## Good example

```csharp
// CORRECT — request DTO contains only client-supplied fields.
// OwnerId and GroupId have no slot on the DTO; the client cannot supply them.

// In {Module}.Api/Requests/:
public record CreateGroupRequest(string Name, string? Description);

// In {Module}.Core:
internal record CreateGroupCommand(Guid GroupId, Guid OwnerId, string Name, string? Description) : ICommand;

// Endpoint:
endpoints.MapPost("/management-groups", async (
    CreateGroupRequest request,   // <-- JSON binder only sees client fields
    IDispatcher dispatcher,
    HttpContext context) =>
{
    var command = new CreateGroupCommand(
        GroupId: Guid.CreateVersion7(),
        OwnerId: GetUserId(context),
        Name: request.Name,
        Description: request.Description);
    await dispatcher.SendAsync(command);
    return Results.Created($"/management-groups/{command.GroupId}", new { id = command.GroupId });
}).RequireAuthorization();
```
