using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.ManagementGroups.Core.DAL;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroups;

internal sealed class GetManagementGroupsHandler : IQueryHandler<GetManagementGroupsQuery, IEnumerable<ManagementGroupDto>>
{
    private readonly ManagementGroupsDbContext _dbContext;

    public GetManagementGroupsHandler(ManagementGroupsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<ManagementGroupDto>> HandleAsync(GetManagementGroupsQuery query, CancellationToken cancellationToken = default)
    {
        var groups = await _dbContext.ManagementGroups
            .Where(x => x.OwnerId == query.UserId || x.Members.Any(m => m.UserId == query.UserId))
            .Select(x => new ManagementGroupDto(x.Id, x.Name, x.Description, x.OwnerId, x.CreatedAt))
            .ToListAsync(cancellationToken);

        return groups;
    }
}
