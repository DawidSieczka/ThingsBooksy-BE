using System;
using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.Core.Domain;
using ThingsBooksy.Shared.Abstractions.DataProviders;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.UpdateManagementGroup.DataProviders;

internal interface IUpdateManagementGroupCommandDataProvider : IDataProvider
{
    Task<ManagementGroup?> GetByIdAsync(Guid groupId, CancellationToken ct);
    Task<bool> OwnerNameExistsForOtherGroupAsync(Guid ownerId, string name, Guid excludedGroupId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
