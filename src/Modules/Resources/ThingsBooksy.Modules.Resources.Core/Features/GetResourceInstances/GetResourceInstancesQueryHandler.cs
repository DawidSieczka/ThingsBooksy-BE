using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.Resources.Core.Exceptions;
using ThingsBooksy.Modules.Resources.Core.Features.GetResourceInstance.Models;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.Resources.Core.Features.GetResourceInstances;

internal sealed class GetResourceInstancesQueryHandler : IQueryHandler<GetResourceInstancesQuery, IReadOnlyList<GetResourceInstancesQueryResult>>
{
    private readonly IGetResourceInstancesQueryDataProvider _dataProvider;

    public GetResourceInstancesQueryHandler(IGetResourceInstancesQueryDataProvider dataProvider)
        => _dataProvider = dataProvider;

    public async Task<IReadOnlyList<GetResourceInstancesQueryResult>> HandleAsync(GetResourceInstancesQuery query, CancellationToken cancellationToken = default)
    {
        Guid resolvedGroupId;

        if (query.GroupId.HasValue)
        {
            resolvedGroupId = query.GroupId.Value;
        }
        else if (query.ResourceTypeId.HasValue)
        {
            var resourceType = await _dataProvider.GetResourceTypeAsync(query.ResourceTypeId.Value, cancellationToken);

            if (resourceType is null)
                throw new ResourcesDomainException("Resource type not found.");

            resolvedGroupId = resourceType.GroupId;
        }
        else
        {
            throw new ResourcesDomainException("Either GroupId or ResourceTypeId must be provided.");
        }

        var isOwner = await _dataProvider.IsOwnerAsync(resolvedGroupId, query.RequesterId, cancellationToken);
        var isMember = !isOwner && await _dataProvider.IsMemberAsync(resolvedGroupId, query.RequesterId, cancellationToken);

        if (!isOwner && !isMember)
            throw new ResourcesForbiddenException("Access to this group is forbidden.");

        var instances = await _dataProvider.GetInstancesAsync(query.ResourceTypeId, query.GroupId, query.IncludeDeleted, cancellationToken);

        if (instances.Count == 0)
            return [];

        var relevantTypeIds = instances.Select(i => i.ResourceTypeId).Distinct();
        var definitions = await _dataProvider.GetPropertyDefinitionsAsync(relevantTypeIds, cancellationToken);
        var defMap = definitions.ToDictionary(d => d.Id);

        return instances
            .Select(instance =>
            {
                var propertyValues = instance.PropertyValues
                    .Select(pv =>
                    {
                        defMap.TryGetValue(pv.PropertyDefinitionId, out var def);
                        return new PropertyValueResult(
                            pv.PropertyDefinitionId,
                            def?.Name ?? string.Empty,
                            def?.DataType.ToString() ?? string.Empty,
                            pv.Value);
                    })
                    .ToList();

                return new GetResourceInstancesQueryResult(
                    instance.Id,
                    instance.ResourceTypeId,
                    instance.GroupId,
                    instance.Name,
                    instance.Description,
                    instance.OwnerId,
                    instance.CreatedAt,
                    instance.DeletedAt,
                    propertyValues);
            })
            .ToList();
    }
}
