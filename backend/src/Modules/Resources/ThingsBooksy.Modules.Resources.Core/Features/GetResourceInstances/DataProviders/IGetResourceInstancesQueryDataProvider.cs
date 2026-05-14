using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.Resources.Core.Domain;
using ThingsBooksy.Shared.Abstractions.DataProviders;

namespace ThingsBooksy.Modules.Resources.Core.Features.GetResourceInstances;

internal interface IGetResourceInstancesQueryDataProvider : IDataProvider
{
    Task<bool> IsOwnerAsync(Guid groupId, Guid userId, CancellationToken ct);
    Task<bool> IsMemberAsync(Guid groupId, Guid userId, CancellationToken ct);
    Task<ResourceType?> GetResourceTypeAsync(Guid resourceTypeId, CancellationToken ct);
    Task<List<ResourceInstance>> GetInstancesAsync(Guid? resourceTypeId, Guid? groupId, bool includeDeleted, Guid? afterId, int take, CancellationToken ct);
    Task<List<ResourcePropertyDefinition>> GetPropertyDefinitionsAsync(IEnumerable<Guid> resourceTypeIds, CancellationToken ct);
}
