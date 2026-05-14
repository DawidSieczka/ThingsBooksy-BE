using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ThingsBooksy.Modules.ManagementGroups.Core.DAL;
using ThingsBooksy.Modules.ManagementGroups.Core.Exceptions;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.AddGroupMember;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.CreateManagementGroup;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.DeleteManagementGroup;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.GetGroupMembers;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroup;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroups;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.IsGroupNameAvailable;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.RemoveGroupMember;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.RestoreManagementGroup;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.UpdateManagementGroup;
using ThingsBooksy.Shared.Abstractions.Commands;
using ThingsBooksy.Shared.Abstractions.Exceptions;
using ThingsBooksy.Shared.Abstractions.Queries;
using ThingsBooksy.Shared.Infrastructure.DataProviders;
using ThingsBooksy.Shared.Infrastructure.Messaging.Outbox;
using ThingsBooksy.Shared.Infrastructure.Postgres;

[assembly: InternalsVisibleTo("ThingsBooksy.Modules.ManagementGroups.Api")]
[assembly: InternalsVisibleTo("ThingsBooksy.Modules.ManagementGroups.Migrations")]
[assembly: InternalsVisibleTo("ThingsBooksy.Modules.ManagementGroups.IntegrationTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace ThingsBooksy.Modules.ManagementGroups.Core;

internal static class Extensions
{
    public static IServiceCollection AddManagementGroupsCore(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddSingleton<IExceptionToResponseMapper, ManagementGroupsExceptionToResponseMapper>()
            .AddScoped<ICommandHandler<CreateManagementGroupCommand>, CreateManagementGroupCommandHandler>()
            .AddScoped<ICommandHandler<UpdateManagementGroupCommand>, UpdateManagementGroupCommandHandler>()
            .AddScoped<ICommandHandler<DeleteManagementGroupCommand>, DeleteManagementGroupCommandHandler>()
            .AddScoped<ICommandHandler<RestoreManagementGroupCommand>, RestoreManagementGroupCommandHandler>()
            .AddScoped<ICommandHandler<AddGroupMemberCommand>, AddGroupMemberCommandHandler>()
            .AddScoped<ICommandHandler<RemoveGroupMemberCommand>, RemoveGroupMemberCommandHandler>()
            .AddScoped<IQueryHandler<GetManagementGroupsQuery, IEnumerable<GetManagementGroupsQueryResult>>, GetManagementGroupsQueryHandler>()
            .AddScoped<IQueryHandler<GetManagementGroupQuery, GetManagementGroupQueryResult?>, GetManagementGroupQueryHandler>()
            .AddScoped<IQueryHandler<IsGroupNameAvailableQuery, IsGroupNameAvailableQueryResult>, IsGroupNameAvailableQueryHandler>()
            .AddScoped<IQueryHandler<GetGroupMembersQuery, GetGroupMembersQueryResult>, GetGroupMembersQueryHandler>()
            .AddDataProviders([typeof(Extensions).Assembly])
            .AddPostgres<ManagementGroupsDbContext>(configuration, "ThingsBooksy.Modules.ManagementGroups.Migrations")
            .AddOutbox<ManagementGroupsDbContext>(configuration)
            .AddUnitOfWork<ManagementGroupsUnitOfWork>();
    }
}
