using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ThingsBooksy.Modules.Users.Core.DAL;
using ThingsBooksy.Modules.Users.Core.Features.GetUser;
using ThingsBooksy.Modules.Users.Core.Features.SignIn;
using ThingsBooksy.Modules.Users.Core.Features.SignUp;
using ThingsBooksy.Modules.Users.Core.Services;
using ThingsBooksy.Shared.Infrastructure;
using ThingsBooksy.Shared.Infrastructure.Messaging.Outbox;
using ThingsBooksy.Shared.Infrastructure.Postgres;

[assembly: InternalsVisibleTo("ThingsBooksy.Modules.Users.Api")]
[assembly: InternalsVisibleTo("ThingsBooksy.Modules.Users.Migrations")]
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
            // Per-feature repositories
            .AddScoped<ISignUpRepository, SignUpRepository>()
            .AddScoped<ISignInRepository, SignInRepository>()
            .AddScoped<IGetUserRepository, GetUserRepository>()
            // Infrastructure
            .AddPostgres<UsersDbContext>(configuration)
            .AddOutbox<UsersDbContext>(configuration)
            .AddUnitOfWork<UsersUnitOfWork>()
            .AddInitializer<UsersInitializer>();
    }
}
