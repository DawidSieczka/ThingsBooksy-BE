using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.Resources.Core.DAL;
using ThingsBooksy.Modules.Resources.Core.Domain;
using ThingsBooksy.Modules.Resources.Core.Exceptions;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.Resources.Core.Features.GetResourceInstances;

internal sealed class GetResourceInstancesHandler : IQueryHandler<GetResourceInstancesQuery, IReadOnlyList<ResourceInstanceDto>>
{
    private readonly ResourcesDbContext _dbContext;

    public GetResourceInstancesHandler(ResourcesDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<IReadOnlyList<ResourceInstanceDto>> HandleAsync(GetResourceInstancesQuery query, CancellationToken cancellationToken = default)
    {
        Guid resolvedGroupId;

        if (query.GroupId.HasValue)
        {
            resolvedGroupId = query.GroupId.Value;
        }
        else if (query.ResourceTypeId.HasValue)
        {
            var resourceType = await _dbContext.ResourceTypes
                .FirstOrDefaultAsync(t => t.Id == query.ResourceTypeId.Value, cancellationToken);

            if (resourceType is null)
                throw new ResourcesDomainException("Resource type not found.");

            resolvedGroupId = resourceType.GroupId;
        }
        else
        {
            throw new ResourcesDomainException("Either GroupId or ResourceTypeId must be provided.");
        }

        var isOwner = await _dbContext.GroupReadModels
            .AnyAsync(g => g.Id == resolvedGroupId && g.OwnerId == query.RequesterId, cancellationToken);

        var isMember = !isOwner && await _dbContext.GroupMemberReadModels
            .AnyAsync(m => m.GroupId == resolvedGroupId && m.UserId == query.RequesterId, cancellationToken);

        if (!isOwner && !isMember)
            throw new ResourcesForbiddenException("Access to this group is forbidden.");

        IQueryable<ResourceInstance> instancesQuery = query.IncludeDeleted
            ? _dbContext.ResourceInstances.IgnoreQueryFilters().Include(x => x.PropertyValues)
            : _dbContext.ResourceInstances.Include(x => x.PropertyValues);

        if (query.ResourceTypeId.HasValue)
            instancesQuery = instancesQuery.Where(x => x.ResourceTypeId == query.ResourceTypeId.Value);

        if (query.GroupId.HasValue)
            instancesQuery = instancesQuery.Where(x => x.GroupId == query.GroupId.Value);

        var instances = await instancesQuery.ToListAsync(cancellationToken);

        if (instances.Count == 0)
            return [];

        var relevantTypeIds = instances.Select(i => i.ResourceTypeId).Distinct().ToList();

        var definitions = await _dbContext.ResourcePropertyDefinitions
            .Where(d => relevantTypeIds.Contains(d.ResourceTypeId))
            .ToListAsync(cancellationToken);

        var defMap = definitions.ToDictionary(d => d.Id);

        return instances
            .Select(instance =>
            {
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
            })
            .ToList();
    }
}
