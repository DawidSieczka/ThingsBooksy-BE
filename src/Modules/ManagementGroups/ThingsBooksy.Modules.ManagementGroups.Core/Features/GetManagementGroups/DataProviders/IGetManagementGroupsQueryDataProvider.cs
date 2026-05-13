using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Shared.Abstractions.DataProviders;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroups.DataProviders;

internal interface IGetManagementGroupsQueryDataProvider : IDataProvider
{
    Task<List<GetManagementGroupsQueryResult>> GetForUserAsync(Guid userId, CancellationToken ct);
}
