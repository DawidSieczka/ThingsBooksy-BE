using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.Core.Exceptions;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.GetGroupMembers.DataProviders;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.GetGroupMembers;

internal sealed class GetGroupMembersQueryHandler : IQueryHandler<GetGroupMembersQuery, GetGroupMembersQueryResult>
{
    private const int MinTake = 1;
    private const int MaxTake = 50;

    private readonly IGetGroupMembersQueryDataProvider _provider;

    public GetGroupMembersQueryHandler(IGetGroupMembersQueryDataProvider provider)
        => _provider = provider;

    public async Task<GetGroupMembersQueryResult> HandleAsync(GetGroupMembersQuery query, CancellationToken cancellationToken = default)
    {
        var group = await _provider.GetGroupSummaryAsync(query.GroupId, cancellationToken)
            ?? throw new ManagementGroupNotFoundException($"Group '{query.GroupId}' was not found.");

        var isOwner = group.OwnerId == query.CallerUserId;
        var isMember = !isOwner && await _provider.IsMemberAsync(query.GroupId, query.CallerUserId, cancellationToken);

        if (!isOwner && !isMember)
            throw new ManagementGroupsForbiddenException("Access to this group is forbidden.");

        var take = Math.Clamp(query.Take, MinTake, MaxTake);

        List<GroupMemberDto> items;

        if (query.AfterId is null)
        {
            // First page: prepend owner row, then fetch members
            var memberPage = await _provider.GetMembersPageAsync(query.GroupId, null, take, cancellationToken);
            var ownerEmail = await _provider.GetOwnerEmailAsync(group.OwnerId, cancellationToken) ?? string.Empty;
            var ownerDto = new GroupMemberDto(Guid.Empty, group.OwnerId, ownerEmail, group.CreatedAt, true);

            items = new List<GroupMemberDto>(memberPage.Count + 1) { ownerDto };
            items.AddRange(memberPage);

            // Trim to take after prepending owner
            if (items.Count > take)
                items = items.GetRange(0, take);

            // NextCursor is the MemberId of the last non-owner item in the page
            var lastMember = items.FindLast(m => !m.IsOwner);
            Guid? nextCursor = lastMember is not null && memberPage.Count >= take
                ? lastMember.MemberId
                : null;

            return new GetGroupMembersQueryResult(items, nextCursor);
        }
        else
        {
            // Subsequent pages: owner is NOT prepended; query members after cursor
            var memberPage = await _provider.GetMembersPageAsync(query.GroupId, query.AfterId, take, cancellationToken);
            items = memberPage;

            Guid? nextCursor = items.Count == take ? items[^1].MemberId : null;
            return new GetGroupMembersQueryResult(items, nextCursor);
        }
    }
}
