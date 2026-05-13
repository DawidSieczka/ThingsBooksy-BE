using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroups.DataProviders;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroups;

internal sealed class GetManagementGroupsQueryHandler : IQueryHandler<GetManagementGroupsQuery, IEnumerable<GetManagementGroupsQueryResult>>
{
    private readonly IGetManagementGroupsQueryDataProvider _provider;

    public GetManagementGroupsQueryHandler(IGetManagementGroupsQueryDataProvider provider)
        => _provider = provider;

    public async Task<IEnumerable<GetManagementGroupsQueryResult>> HandleAsync(GetManagementGroupsQuery query, CancellationToken cancellationToken = default)
        => await _provider.GetForUserAsync(query.UserId, cancellationToken);
}
