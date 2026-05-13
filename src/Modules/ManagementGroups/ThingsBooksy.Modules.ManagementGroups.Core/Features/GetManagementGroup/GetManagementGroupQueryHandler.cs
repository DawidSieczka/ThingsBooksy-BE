using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.Core.Exceptions;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroup.DataProviders;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.GetManagementGroup;

internal sealed class GetManagementGroupQueryHandler : IQueryHandler<GetManagementGroupQuery, GetManagementGroupQueryResult?>
{
    private readonly IGetManagementGroupQueryDataProvider _provider;

    public GetManagementGroupQueryHandler(IGetManagementGroupQueryDataProvider provider)
        => _provider = provider;

    public async Task<GetManagementGroupQueryResult?> HandleAsync(GetManagementGroupQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _provider.GetByIdAsync(query.GroupId, cancellationToken);

        if (result is null)
            return null;

        var isOwner = result.OwnerId == query.RequesterId;
        var isMember = result.Members.Any(m => m.UserId == query.RequesterId);

        if (!isOwner && !isMember)
            throw new ManagementGroupsForbiddenException("Access to this group is forbidden.");

        return result;
    }
}
