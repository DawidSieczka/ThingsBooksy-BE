using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.ManagementGroups.Core.DAL;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.GetGroupMembers.DataProviders;

internal sealed class GetGroupMembersQueryDataProvider : IGetGroupMembersQueryDataProvider
{
    private readonly ManagementGroupsDbContext _db;

    public GetGroupMembersQueryDataProvider(ManagementGroupsDbContext db)
        => _db = db;

    public async Task<GroupSummary?> GetGroupSummaryAsync(Guid groupId, CancellationToken ct)
    {
        var raw = await (from g in _db.ManagementGroups
                         where g.Id == groupId
                         select new { g.Id, g.OwnerId, g.CreatedAt })
                        .FirstOrDefaultAsync(ct);

        return raw is null
            ? null
            : new GroupSummary(raw.Id, raw.OwnerId, new DateTimeOffset(raw.CreatedAt, TimeSpan.Zero));
    }

    public Task<bool> IsMemberAsync(Guid groupId, Guid userId, CancellationToken ct)
        => _db.GroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == userId, ct);

    public async Task<List<GroupMemberDto>> GetMembersPageAsync(Guid groupId, Guid? afterId, int take, CancellationToken ct)
    {
        var rows = await (from m in _db.GroupMembers
                          join u in _db.UserReadModels on m.UserId equals u.Id
                          where m.GroupId == groupId && (afterId == null || m.UserId > afterId.Value)
                          orderby m.UserId
                          select new { m.UserId, u.Email, m.JoinedAt })
                         .Take(take)
                         .ToListAsync(ct);

        return rows
            .Select(r => new GroupMemberDto(r.UserId, r.UserId, r.Email, new DateTimeOffset(r.JoinedAt, TimeSpan.Zero), false))
            .ToList();
    }

    public Task<string?> GetOwnerEmailAsync(Guid ownerId, CancellationToken ct)
        => (from u in _db.UserReadModels
            where u.Id == ownerId
            select u.Email)
           .FirstOrDefaultAsync(ct);
}
