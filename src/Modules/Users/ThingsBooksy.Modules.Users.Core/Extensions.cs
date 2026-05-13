using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ThingsBooksy.Modules.Users.Core.DAL;
using ThingsBooksy.Modules.Users.Core.Features.GetUser;
using ThingsBooksy.Modules.Users.Core.Features.SignIn;
using ThingsBooksy.Modules.Users.Core.Features.SignUp;
using ThingsBooksy.Modules.Users.Core.Services;
using ThingsBooksy.Shared.Abstractions.Commands;
using ThingsBooksy.Shared.Abstractions.Queries;
using ThingsBooksy.Shared.Infrastructure;
using ThingsBooksy.Shared.Infrastructure.DataProviders;
using ThingsBooksy.Shared.Infrastructure.Messaging.Outbox;
using ThingsBooksy.Shared.Infrastructure.Postgres;

[assembly: InternalsVisibleTo("ThingsBooksy.Modules.Users.Api")]
[assembly: InternalsVisibleTo("ThingsBooksy.Modules.Users.Migrations")]
[assembly: InternalsVisibleTo("ThingsBooksy.Modules.Users.IntegrationTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace ThingsBooksy.Modules.Users.Core;

internal static class Extensions
{
    public static IServiceCollection AddUsersCore(this IServiceCollection services, IConfiguration configuration)
    {
        var registrationSection = configuration.GetSection("users:registration");
        services.Configure<RegistrationOptions>(registrationSection);

        return services
            .AddSingleton<ITokenStorage, HttpContextTokenStorage>()
            // Command handlers
            .AddScoped<ICommandHandler<SignUpCommand>, SignUpHandler>()
            .AddScoped<ICommandHandler<SignInCommand>, SignInHandler>()
            // Query handlers
            .AddScoped<IQueryHandler<GetUserQuery, GetUserQueryResult?>, GetUserQueryHandler>()
            // Data providers
            .AddDataProviders([typeof(Extensions).Assembly])
            // Infrastructure
            .AddPostgres<UsersDbContext>(configuration, "ThingsBooksy.Modules.Users.Migrations")
            .AddOutbox<UsersDbContext>(configuration)
            .AddUnitOfWork<UsersUnitOfWork>()
            .AddInitializer<UsersInitializer>();
    }
}
