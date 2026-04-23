using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ThingsBooksy.Modules.ManagementGroups.Core.DAL;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.AddGroupMember;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.CreateManagementGroup;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.DeleteManagementGroup;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroup;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroups;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.RemoveGroupMember;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.RestoreManagementGroup;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.UpdateManagementGroup;
using ThingsBooksy.Shared.Abstractions.Commands;
using ThingsBooksy.Shared.Abstractions.Queries;
using ThingsBooksy.Shared.Infrastructure.Messaging.Outbox;
using ThingsBooksy.Shared.Infrastructure.Postgres;

[assembly: InternalsVisibleTo("ThingsBooksy.Modules.ManagementGroups.Api")]
[assembly: InternalsVisibleTo("ThingsBooksy.Modules.ManagementGroups.Migrations")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace ThingsBooksy.Modules.ManagementGroups.Core;

internal static class Extensions
{
    public static IServiceCollection AddManagementGroupsCore(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddScoped<ICommandHandler<CreateManagementGroupCommand>, CreateManagementGroupHandler>()
            .AddScoped<ICommandHandler<UpdateManagementGroupCommand>, UpdateManagementGroupHandler>()
            .AddScoped<ICommandHandler<DeleteManagementGroupCommand>, DeleteManagementGroupHandler>()
            .AddScoped<ICommandHandler<RestoreManagementGroupCommand>, RestoreManagementGroupHandler>()
            .AddScoped<ICommandHandler<AddGroupMemberCommand>, AddGroupMemberHandler>()
            .AddScoped<ICommandHandler<RemoveGroupMemberCommand>, RemoveGroupMemberHandler>()
            .AddScoped<IQueryHandler<GetManagementGroupsQuery, IEnumerable<ManagementGroupDto>>, GetManagementGroupsHandler>()
            .AddScoped<IQueryHandler<GetManagementGroupQuery, ManagementGroupDetailDto?>, GetManagementGroupHandler>()
            .AddPostgres<ManagementGroupsDbContext>(configuration, "ThingsBooksy.Modules.ManagementGroups.Migrations")
            .AddOutbox<ManagementGroupsDbContext>(configuration)
            .AddUnitOfWork<ManagementGroupsUnitOfWork>();
    }
}
