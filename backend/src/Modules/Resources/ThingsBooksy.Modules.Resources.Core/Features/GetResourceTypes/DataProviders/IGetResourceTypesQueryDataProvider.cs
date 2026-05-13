using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Shared.Abstractions.DataProviders;

namespace ThingsBooksy.Modules.Resources.Core.Features.GetResourceTypes;

internal interface IGetResourceTypesQueryDataProvider : IDataProvider
{
    Task<bool> IsOwnerAsync(Guid groupId, Guid userId, CancellationToken ct);
    Task<bool> IsMemberAsync(Guid groupId, Guid userId, CancellationToken ct);
    Task<List<GetResourceTypesQueryResult>> GetByGroupIdAsync(Guid groupId, CancellationToken ct);
}
