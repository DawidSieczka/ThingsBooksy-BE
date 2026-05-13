using System;
using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.Core.Domain;
using ThingsBooksy.Modules.ManagementGroups.Core.ReadModels;
using ThingsBooksy.Shared.Abstractions.DataProviders;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.AddGroupMember.DataProviders;

internal interface IAddGroupMemberCommandDataProvider : IDataProvider
{
    Task<ManagementGroup?> GetGroupWithMembersAsync(Guid groupId, CancellationToken ct);
    Task<UserReadModel?> GetUserByEmailAsync(string email, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
