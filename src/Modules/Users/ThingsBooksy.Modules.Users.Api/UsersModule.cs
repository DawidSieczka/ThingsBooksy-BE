using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ThingsBooksy.Modules.Users.Core;
using ThingsBooksy.Modules.Users.Core.Features.GetUser;
using ThingsBooksy.Modules.Users.Core.Features.SignIn;
using ThingsBooksy.Modules.Users.Core.Features.SignUp;
using ThingsBooksy.Modules.Users.Core.Services;
using ThingsBooksy.Shared.Abstractions.Dispatchers;
using ThingsBooksy.Shared.Abstractions.Modules;

namespace ThingsBooksy.Modules.Users.Api;

internal sealed class UsersModule : IModule
{
    public string Name { get; } = "Users";

    public IEnumerable<string> Policies { get; } = ["users"];

    public void Register(IServiceCollection services, IConfiguration configuration)
        => services.AddUsersCore(configuration);

    public void Use(IApplicationBuilder app)
    {
    }

    public void Expose(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/users/{id:guid}", async (Guid id, IDispatcher dispatcher) =>
        {
            var user = await dispatcher.QueryAsync(new GetUserQuery { UserId = id });
            return user is null ? Results.NotFound() : Results.Ok(user);
        }).WithTags("Users").WithName("Get user");

        endpoints.MapGet("/me", async (IDispatcher dispatcher, HttpContext context) =>
        {
            var userId = GetUserId(context);
            var user = await dispatcher.QueryAsync(new GetUserQuery { UserId = userId });
            return user is null ? Results.NotFound() : Results.Ok(user);
        }).RequireAuthorization().WithTags("Account").WithName("Get account");

        endpoints.MapPost("/sign-up", async (SignUpCommand command, IDispatcher dispatcher) =>
        {
            await dispatcher.SendAsync(command with { UserId = Guid.NewGuid() });
            return Results.NoContent();
        }).WithTags("Account").WithName("Sign up");

        endpoints.MapPost("/sign-in", async (SignInCommand command, IDispatcher dispatcher, ITokenStorage storage) =>
        {
            await dispatcher.SendAsync(command);
            var jwt = storage.Get();
            return Results.Ok(jwt);
        }).WithTags("Account").WithName("Sign in");
    }

    private static Guid GetUserId(HttpContext context)
        => string.IsNullOrWhiteSpace(context.User.Identity?.Name)
            ? Guid.Empty
            : Guid.Parse(context.User.Identity.Name);
}
