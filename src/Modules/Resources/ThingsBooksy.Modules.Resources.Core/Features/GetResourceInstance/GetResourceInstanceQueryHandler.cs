using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.Resources.Core.Exceptions;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.Resources.Core.Features.GetResourceInstance;

internal sealed class GetResourceInstanceQueryHandler : IQueryHandler<GetResourceInstanceQuery, GetResourceInstanceQueryResult?>
{
    private readonly IGetResourceInstanceQueryDataProvider _dataProvider;

    public GetResourceInstanceQueryHandler(IGetResourceInstanceQueryDataProvider dataProvider)
        => _dataProvider = dataProvider;

    public async Task<GetResourceInstanceQueryResult?> HandleAsync(GetResourceInstanceQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _dataProvider.GetByIdAsync(query.InstanceId, cancellationToken);

        if (result is null)
            return null;

        var isOwner = await _dataProvider.IsOwnerAsync(result.GroupId, query.RequesterId, cancellationToken);
        var isMember = !isOwner && await _dataProvider.IsMemberAsync(result.GroupId, query.RequesterId, cancellationToken);

        if (!isOwner && !isMember)
            throw new ResourcesForbiddenException("Access to this group is forbidden.");

        return result;
    }
}
