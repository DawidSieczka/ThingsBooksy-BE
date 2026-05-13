using System;
using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.Resources.Core.Domain;
using ThingsBooksy.Shared.Abstractions.DataProviders;

namespace ThingsBooksy.Modules.Resources.Core.Features.DeleteResourceInstance;

internal interface IDeleteResourceInstanceCommandDataProvider : IDataProvider
{
    Task<ResourceInstance?> GetInstanceAsync(Guid instanceId, CancellationToken ct);
    Task<bool> IsOwnerAsync(Guid groupId, Guid userId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
