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

    public Task<GetManagementGroupQueryResult?> GetByIdAsync(Guid groupId, CancellationToken ct)
        => (from g in _dbContext.ManagementGroups
            where g.Id == groupId
            select new GetManagementGroupQueryResult(
                g.Id,
                g.Name,
                g.Description,
                g.OwnerId,
                g.CreatedAt,
                g.Members.Select(m => new ManagementGroupMemberResult(m.UserId, m.JoinedAt)).ToList()))
           .FirstOrDefaultAsync(ct);
}
