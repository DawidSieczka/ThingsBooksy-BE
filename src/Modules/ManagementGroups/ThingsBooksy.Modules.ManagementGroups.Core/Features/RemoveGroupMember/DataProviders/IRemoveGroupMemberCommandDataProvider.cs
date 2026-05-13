using System;
using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.Core.Domain;
using ThingsBooksy.Shared.Abstractions.DataProviders;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.RemoveGroupMember.DataProviders;

internal interface IRemoveGroupMemberCommandDataProvider : IDataProvider
{
    Task<ManagementGroup?> GetGroupWithMembersAsync(Guid groupId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
