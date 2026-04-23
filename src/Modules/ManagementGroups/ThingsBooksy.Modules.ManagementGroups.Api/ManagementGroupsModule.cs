using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ThingsBooksy.Modules.ManagementGroups.Core;
using ThingsBooksy.Modules.ManagementGroups.Core.Events.Handlers;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.AddGroupMember;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.CreateManagementGroup;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.DeleteManagementGroup;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroup;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroups;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.RemoveGroupMember;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.RestoreManagementGroup;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.UpdateManagementGroup;
using ThingsBooksy.Shared.Abstractions.Dispatchers;
using ThingsBooksy.Shared.Abstractions.Events;
using ThingsBooksy.Shared.Abstractions.Modules;

namespace ThingsBooksy.Modules.ManagementGroups.Api;

internal sealed class ManagementGroupsModule : IModule
{
    public string Name { get; } = "ManagementGroups";
    public IEnumerable<string> Policies { get; } = ["managementgroups"];

    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddManagementGroupsCore(configuration);
        services.AddScoped<IEventHandler<UserSignedUp>, UserSignedUpHandler>();
    }

    public void Use(IApplicationBuilder app) { }

    public void Expose(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/management-groups", async (CreateManagementGroupRequest request, IDispatcher dispatcher, HttpContext context) =>
        {
            var userId = GetUserId(context);
            var command = new CreateManagementGroupCommand(Guid.CreateVersion7(), request.Name, request.Description, userId);
            await dispatcher.SendAsync(command);
            return Results.Created($"/management-groups/{command.GroupId}", new { id = command.GroupId });
        }).RequireAuthorization().WithTags("ManagementGroups").WithName("Create management group");

        endpoints.MapGet("/management-groups", async (IDispatcher dispatcher, HttpContext context) =>
        {
            var userId = GetUserId(context);
            var groups = await dispatcher.QueryAsync(new GetManagementGroupsQuery(userId));
            return Results.Ok(groups);
        }).RequireAuthorization().WithTags("ManagementGroups").WithName("Get management groups");

        endpoints.MapGet("/management-groups/{id:guid}", async (Guid id, IDispatcher dispatcher, HttpContext context) =>
        {
            var userId = GetUserId(context);
            var group = await dispatcher.QueryAsync(new GetManagementGroupQuery(id, userId));
            return group is null ? Results.NotFound() : Results.Ok(group);
        }).RequireAuthorization().WithTags("ManagementGroups").WithName("Get management group");

        endpoints.MapPut("/management-groups/{id:guid}", async (Guid id, UpdateManagementGroupRequest request, IDispatcher dispatcher, HttpContext context) =>
        {
            var userId = GetUserId(context);
            await dispatcher.SendAsync(new UpdateManagementGroupCommand(id, request.Name, request.Description, userId));
            return Results.Ok();
        }).RequireAuthorization().WithTags("ManagementGroups").WithName("Update management group");

        endpoints.MapDelete("/management-groups/{id:guid}", async (Guid id, IDispatcher dispatcher, HttpContext context) =>
        {
            var userId = GetUserId(context);
            await dispatcher.SendAsync(new DeleteManagementGroupCommand(id, userId));
            return Results.NoContent();
        }).RequireAuthorization().WithTags("ManagementGroups").WithName("Delete management group");

        endpoints.MapPost("/management-groups/{id:guid}/restore", async (Guid id, IDispatcher dispatcher, HttpContext context) =>
        {
            var userId = GetUserId(context);
            await dispatcher.SendAsync(new RestoreManagementGroupCommand(id, userId));
            return Results.Ok();
        }).RequireAuthorization().WithTags("ManagementGroups").WithName("Restore management group");

        endpoints.MapPost("/management-groups/{id:guid}/members", async (Guid id, AddGroupMemberRequest request, IDispatcher dispatcher, HttpContext context) =>
        {
            var userId = GetUserId(context);
            await dispatcher.SendAsync(new AddGroupMemberCommand(id, request.Email, userId));
            return Results.Created();
        }).RequireAuthorization().WithTags("ManagementGroups").WithName("Add group member");

        endpoints.MapDelete("/management-groups/{id:guid}/members/{memberId:guid}", async (Guid id, Guid memberId, IDispatcher dispatcher, HttpContext context) =>
        {
            var userId = GetUserId(context);
            await dispatcher.SendAsync(new RemoveGroupMemberCommand(id, memberId, userId));
            return Results.NoContent();
        }).RequireAuthorization().WithTags("ManagementGroups").WithName("Remove group member");
    }

    private static Guid GetUserId(HttpContext context)
        => string.IsNullOrWhiteSpace(context.User.Identity?.Name)
            ? Guid.Empty
            : Guid.Parse(context.User.Identity.Name);
}

public record CreateManagementGroupRequest(string Name, string? Description);
public record UpdateManagementGroupRequest(string Name, string? Description);
public record AddGroupMemberRequest(string Email);
