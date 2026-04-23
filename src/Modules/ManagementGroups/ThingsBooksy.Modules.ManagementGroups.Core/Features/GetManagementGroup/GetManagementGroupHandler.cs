using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.ManagementGroups.Core.DAL;
using ThingsBooksy.Modules.ManagementGroups.Core.Exceptions;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroup;

internal sealed class GetManagementGroupHandler : IQueryHandler<GetManagementGroupQuery, ManagementGroupDetailDto?>
{
    private readonly ManagementGroupsDbContext _dbContext;

    public GetManagementGroupHandler(ManagementGroupsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ManagementGroupDetailDto?> HandleAsync(GetManagementGroupQuery query, CancellationToken cancellationToken = default)
    {
        var group = await _dbContext.ManagementGroups
            .Include(x => x.Members)
            .FirstOrDefaultAsync(x => x.Id == query.GroupId, cancellationToken);

        if (group is null)
            return null;

        var isOwner = group.OwnerId == query.RequesterId;
        var isMember = group.Members.Any(x => x.UserId == query.RequesterId);

        if (!isOwner && !isMember)
            throw new ManagementGroupsForbiddenException("Access to this group is forbidden.");

        return new ManagementGroupDetailDto(
            group.Id,
            group.Name,
            group.Description,
            group.OwnerId,
            group.CreatedAt,
            group.Members.Select(m => new GroupMemberDto(m.UserId, m.JoinedAt)));
    }
}
