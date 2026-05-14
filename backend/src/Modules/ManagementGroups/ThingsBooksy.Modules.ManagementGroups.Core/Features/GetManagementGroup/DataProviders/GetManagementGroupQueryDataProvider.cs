using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.ManagementGroups.Core.DAL;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroup.Models.Results;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroup.DataProviders;

internal sealed class GetManagementGroupQueryDataProvider : IGetManagementGroupQueryDataProvider
{
    private readonly ManagementGroupsDbContext _dbContext;

    public GetManagementGroupQueryDataProvider(ManagementGroupsDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<GetManagementGroupQueryResult?> GetByIdAsync(Guid groupId, CancellationToken ct)
    {
        var raw = await (from g in _dbContext.ManagementGroups
                         where g.Id == groupId
                         select new
                         {
                             g.Id,
                             g.Name,
                             g.Description,
                             g.OwnerId,
                             g.CreatedAt,
                             MemberCount = g.Members.Count + 1,
                             Members = g.Members.Select(m => new { m.UserId, m.JoinedAt }).ToList()
                         })
                        .FirstOrDefaultAsync(ct);

        if (raw is null)
            return null;

        return new GetManagementGroupQueryResult(
            raw.Id,
            raw.Name,
            raw.Description,
            raw.OwnerId,
            new DateTimeOffset(raw.CreatedAt, TimeSpan.Zero),
            raw.MemberCount,
            raw.Members.Select(m => new ManagementGroupMemberResult(m.UserId, m.JoinedAt)).ToList());
    }
}
