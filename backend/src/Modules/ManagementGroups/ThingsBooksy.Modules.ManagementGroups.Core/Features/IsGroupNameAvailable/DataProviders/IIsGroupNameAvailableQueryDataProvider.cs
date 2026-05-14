using System;
using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Shared.Abstractions.DataProviders;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.IsGroupNameAvailable.DataProviders;

internal interface IIsGroupNameAvailableQueryDataProvider : IDataProvider
{
    Task<bool> ExistsAsync(Guid ownerId, string name, CancellationToken ct);
}
