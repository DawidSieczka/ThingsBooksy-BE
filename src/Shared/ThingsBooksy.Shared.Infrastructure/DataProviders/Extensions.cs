using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ThingsBooksy.Shared.Abstractions.DataProviders;

namespace ThingsBooksy.Shared.Infrastructure.DataProviders;

public static class Extensions
{
    public static IServiceCollection AddDataProviders(
        this IServiceCollection services,
        IEnumerable<Assembly> assemblies)
    {
        services.Scan(s => s
            .FromAssemblies(assemblies)
            .AddClasses(c => c.AssignableTo<IDataProvider>())
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
