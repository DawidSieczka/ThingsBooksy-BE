using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.ManagementGroups.Core.DAL;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroups.DataProviders;

internal sealed class GetManagementGroupsQueryDataProvider : IGetManagementGroupsQueryDataProvider
{
    private readonly ManagementGroupsDbContext _dbContext;

    public GetManagementGroupsQueryDataProvider(ManagementGroupsDbContext dbContext)
        => _dbContext = dbContext;

    public Task<List<GetManagementGroupsQueryResult>> GetForUserAsync(Guid userId, CancellationToken ct)
        => (from g in _dbContext.ManagementGroups
            where g.OwnerId == userId || g.Members.Any(m => m.UserId == userId)
            select new GetManagementGroupsQueryResult(
                g.Id,
                g.Name,
                g.Description,
                g.OwnerId,
                g.CreatedAt,
                g.Members.Count + 1))
           .ToListAsync(ct);
}
