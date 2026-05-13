using System;
using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Shared.Abstractions.DataProviders;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroup.DataProviders;

internal interface IGetManagementGroupQueryDataProvider : IDataProvider
{
    Task<GetManagementGroupQueryResult?> GetByIdAsync(Guid groupId, CancellationToken ct);
}
