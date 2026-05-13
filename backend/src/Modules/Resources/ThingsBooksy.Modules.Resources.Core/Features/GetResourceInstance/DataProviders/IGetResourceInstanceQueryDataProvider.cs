using System;
using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Shared.Abstractions.DataProviders;

namespace ThingsBooksy.Modules.Resources.Core.Features.GetResourceInstance;

internal interface IGetResourceInstanceQueryDataProvider : IDataProvider
{
    Task<GetResourceInstanceQueryResult?> GetByIdAsync(Guid instanceId, CancellationToken ct);
    Task<bool> IsOwnerAsync(Guid groupId, Guid userId, CancellationToken ct);
    Task<bool> IsMemberAsync(Guid groupId, Guid userId, CancellationToken ct);
}
