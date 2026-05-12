using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.Resources.Core.DAL;
using ThingsBooksy.Modules.Resources.Core.Exceptions;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.Resources.Core.Features.GetResourceInstance;

internal sealed class GetResourceInstanceHandler : IQueryHandler<GetResourceInstanceQuery, ResourceInstanceDto?>
{
    private readonly ResourcesDbContext _dbContext;

    public GetResourceInstanceHandler(ResourcesDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<ResourceInstanceDto?> HandleAsync(GetResourceInstanceQuery query, CancellationToken cancellationToken = default)
    {
        var instance = await _dbContext.ResourceInstances
            .Include(x => x.PropertyValues)
            .FirstOrDefaultAsync(x => x.Id == query.InstanceId, cancellationToken);

        if (instance is null)
            return null;

        var isOwner = await _dbContext.GroupReadModels
            .AnyAsync(g => g.Id == instance.GroupId && g.OwnerId == query.RequesterId, cancellationToken);

        var isMember = !isOwner && await _dbContext.GroupMemberReadModels
            .AnyAsync(m => m.GroupId == instance.GroupId && m.UserId == query.RequesterId, cancellationToken);

        if (!isOwner && !isMember)
            throw new ResourcesForbiddenException("Access to this group is forbidden.");

        var definitions = await _dbContext.ResourcePropertyDefinitions
            .Where(d => d.ResourceTypeId == instance.ResourceTypeId)
            .ToListAsync(cancellationToken);

        var defMap = definitions.ToDictionary(d => d.Id);

        var propertyValueDtos = instance.PropertyValues
            .Select(pv =>
            {
                defMap.TryGetValue(pv.PropertyDefinitionId, out var def);
                return new PropertyValueDto(
                    pv.PropertyDefinitionId,
                    def?.Name ?? string.Empty,
                    def?.DataType.ToString() ?? string.Empty,
                    pv.Value);
            })
            .ToList();

        return new ResourceInstanceDto(
            instance.Id,
            instance.ResourceTypeId,
            instance.GroupId,
            instance.Name,
            instance.Description,
            instance.OwnerId,
            instance.CreatedAt,
            instance.DeletedAt,
            propertyValueDtos);
    }
}
