using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.Resources.Core.DAL;
using ThingsBooksy.Modules.Resources.Core.Exceptions;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.Resources.Core.Features.GetResourceTypes;

internal sealed class GetResourceTypesHandler : IQueryHandler<GetResourceTypesQuery, IReadOnlyList<ResourceTypeDto>>
{
    private readonly ResourcesDbContext _dbContext;

    public GetResourceTypesHandler(ResourcesDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<IReadOnlyList<ResourceTypeDto>> HandleAsync(GetResourceTypesQuery query, CancellationToken cancellationToken = default)
    {
        var isOwner = await _dbContext.GroupReadModels
            .AnyAsync(g => g.Id == query.GroupId && g.OwnerId == query.RequesterId, cancellationToken);

        var isMember = !isOwner && await _dbContext.GroupMemberReadModels
            .AnyAsync(m => m.GroupId == query.GroupId && m.UserId == query.RequesterId, cancellationToken);

        if (!isOwner && !isMember)
            throw new ResourcesForbiddenException("Access to this group is forbidden.");

        var types = await _dbContext.ResourceTypes
            .Include(x => x.PropertyDefinitions)
            .Where(x => x.GroupId == query.GroupId)
            .ToListAsync(cancellationToken);

        return types
            .Select(t => new ResourceTypeDto(
                t.Id,
                t.GroupId,
                t.Name,
                t.Description,
                t.CreatedAt,
                t.PropertyDefinitions
                    .Select(d => new PropertyDefinitionDto(d.Id, d.Name, d.DataType.ToString(), d.IsRequired))
                    .ToList()))
            .ToList();
    }
}
