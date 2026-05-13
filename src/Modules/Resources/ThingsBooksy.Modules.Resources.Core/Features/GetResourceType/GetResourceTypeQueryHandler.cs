using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.Resources.Core.Exceptions;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.Resources.Core.Features.GetResourceType;

internal sealed class GetResourceTypeQueryHandler : IQueryHandler<GetResourceTypeQuery, GetResourceTypeQueryResult?>
{
    private readonly IGetResourceTypeQueryDataProvider _dataProvider;

    public GetResourceTypeQueryHandler(IGetResourceTypeQueryDataProvider dataProvider)
        => _dataProvider = dataProvider;

    public async Task<GetResourceTypeQueryResult?> HandleAsync(GetResourceTypeQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _dataProvider.GetByIdAsync(query.TypeId, cancellationToken);

        if (result is null)
            return null;

        var isOwner = await _dataProvider.IsOwnerAsync(result.GroupId, query.RequesterId, cancellationToken);
        var isMember = !isOwner && await _dataProvider.IsMemberAsync(result.GroupId, query.RequesterId, cancellationToken);

        if (!isOwner && !isMember)
            throw new ResourcesForbiddenException("Access to this group is forbidden.");

        return result;
    }
}
