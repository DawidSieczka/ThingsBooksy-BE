using System;
using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Shared.Abstractions.DataProviders;

namespace ThingsBooksy.Modules.Resources.Core.Features.GetResourceType;

internal interface IGetResourceTypeQueryDataProvider : IDataProvider
{
    Task<GetResourceTypeQueryResult?> GetByIdAsync(Guid typeId, CancellationToken ct);
    Task<bool> IsOwnerAsync(Guid groupId, Guid userId, CancellationToken ct);
    Task<bool> IsMemberAsync(Guid groupId, Guid userId, CancellationToken ct);
}
