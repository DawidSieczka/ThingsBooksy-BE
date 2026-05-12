using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ThingsBooksy.Modules.Resources.Core.DAL;
using ThingsBooksy.Modules.Resources.Core.Features;
using ThingsBooksy.Modules.Resources.Core.Features.CreateResourceInstance;
using ThingsBooksy.Modules.Resources.Core.Features.CreateResourceType;
using ThingsBooksy.Modules.Resources.Core.Features.GetResourceInstance;
using ThingsBooksy.Modules.Resources.Core.Features.GetResourceInstances;
using ThingsBooksy.Modules.Resources.Core.Features.DeleteResourceInstance;
using ThingsBooksy.Modules.Resources.Core.Features.GetResourceType;
using ThingsBooksy.Modules.Resources.Core.Features.GetResourceTypes;
using ThingsBooksy.Modules.Resources.Core.Features.UpdateResourceInstance;
using ThingsBooksy.Modules.Resources.Core.Features.UpdateResourceType;
using ThingsBooksy.Modules.Resources.Core.Features.DeleteResourceType;
using ThingsBooksy.Shared.Abstractions.Commands;
using ThingsBooksy.Shared.Abstractions.Queries;
using ThingsBooksy.Shared.Infrastructure.Messaging.Outbox;
using ThingsBooksy.Shared.Infrastructure.Postgres;

[assembly: InternalsVisibleTo("ThingsBooksy.Modules.Resources.Api")]
[assembly: InternalsVisibleTo("ThingsBooksy.Modules.Resources.Migrations")]
[assembly: InternalsVisibleTo("ThingsBooksy.Modules.Resources.IntegrationTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace ThingsBooksy.Modules.Resources.Core;

internal static class Extensions
{
    public static IServiceCollection AddResourcesCore(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddScoped<ICommandHandler<CreateResourceInstanceCommand>, CreateResourceInstanceHandler>()
            .AddScoped<ICommandHandler<CreateResourceTypeCommand>, CreateResourceTypeHandler>()
            .AddScoped<IQueryHandler<GetResourceTypeQuery, ResourceTypeDto?>, GetResourceTypeHandler>()
            .AddScoped<IQueryHandler<GetResourceTypesQuery, IReadOnlyList<ResourceTypeDto>>, GetResourceTypesHandler>()
            .AddScoped<IQueryHandler<GetResourceInstanceQuery, ResourceInstanceDto?>, GetResourceInstanceHandler>()
            .AddScoped<IQueryHandler<GetResourceInstancesQuery, IReadOnlyList<ResourceInstanceDto>>, GetResourceInstancesHandler>()
            .AddScoped<ICommandHandler<UpdateResourceInstanceCommand>, UpdateResourceInstanceHandler>()
            .AddScoped<ICommandHandler<DeleteResourceInstanceCommand>, DeleteResourceInstanceHandler>()
            .AddScoped<ICommandHandler<UpdateResourceTypeCommand>, UpdateResourceTypeHandler>()
            .AddScoped<ICommandHandler<DeleteResourceTypeCommand>, DeleteResourceTypeHandler>()
            .AddPostgres<ResourcesDbContext>(configuration, "ThingsBooksy.Modules.Resources.Migrations")
            .AddOutbox<ResourcesDbContext>(configuration)
            .AddUnitOfWork<ResourcesUnitOfWork>();
    }
}
