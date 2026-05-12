using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.Resources.Core.DAL;
using ThingsBooksy.Modules.Resources.Core.Exceptions;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.Resources.Core.Features.GetResourceType;

internal sealed class GetResourceTypeHandler : IQueryHandler<GetResourceTypeQuery, ResourceTypeDto?>
{
    private readonly ResourcesDbContext _dbContext;

    public GetResourceTypeHandler(ResourcesDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<ResourceTypeDto?> HandleAsync(GetResourceTypeQuery query, CancellationToken cancellationToken = default)
    {
        var resourceType = await _dbContext.ResourceTypes
            .Include(x => x.PropertyDefinitions)
            .FirstOrDefaultAsync(x => x.Id == query.TypeId, cancellationToken);

        if (resourceType is null)
            return null;

        var isOwner = await _dbContext.GroupReadModels
            .AnyAsync(g => g.Id == resourceType.GroupId && g.OwnerId == query.RequesterId, cancellationToken);

        var isMember = !isOwner && await _dbContext.GroupMemberReadModels
            .AnyAsync(m => m.GroupId == resourceType.GroupId && m.UserId == query.RequesterId, cancellationToken);

        if (!isOwner && !isMember)
            throw new ResourcesForbiddenException("Access to this group is forbidden.");

        return MapToDto(resourceType);
    }

    private static ResourceTypeDto MapToDto(Domain.ResourceType type) => new(
        type.Id,
        type.GroupId,
        type.Name,
        type.Description,
        type.CreatedAt,
        type.PropertyDefinitions
            .Select(d => new PropertyDefinitionDto(d.Id, d.Name, d.DataType.ToString(), d.IsRequired))
            .ToList());
}
