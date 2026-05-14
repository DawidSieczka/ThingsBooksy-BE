using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Shared.Abstractions.DataProviders;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.GetGroupMembers.DataProviders;

internal interface IGetGroupMembersQueryDataProvider : IDataProvider
{
    Task<GroupSummary?> GetGroupSummaryAsync(Guid groupId, CancellationToken ct);
    Task<bool> IsMemberAsync(Guid groupId, Guid userId, CancellationToken ct);
    Task<List<GroupMemberDto>> GetMembersPageAsync(Guid groupId, Guid? afterId, int take, CancellationToken ct);
    Task<string?> GetOwnerEmailAsync(Guid ownerId, CancellationToken ct);
}

internal sealed record GroupSummary(Guid Id, Guid OwnerId, DateTimeOffset CreatedAt);
