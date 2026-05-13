using System;
using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.Core.Domain;
using ThingsBooksy.Shared.Abstractions.DataProviders;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.DeleteManagementGroup.DataProviders;

internal interface IDeleteManagementGroupCommandDataProvider : IDataProvider
{
    Task<ManagementGroup?> GetByIdAsync(Guid groupId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
