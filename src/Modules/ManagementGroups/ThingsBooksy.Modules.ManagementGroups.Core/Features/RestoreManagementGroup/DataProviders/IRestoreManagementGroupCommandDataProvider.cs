using System;
using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.Core.Domain;
using ThingsBooksy.Shared.Abstractions.DataProviders;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.RestoreManagementGroup.DataProviders;

internal interface IRestoreManagementGroupCommandDataProvider : IDataProvider
{
    Task<ManagementGroup?> GetByIdIgnoringFiltersAsync(Guid groupId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
