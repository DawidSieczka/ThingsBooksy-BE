using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.IsGroupNameAvailable.DataProviders;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Features.IsGroupNameAvailable;

internal sealed class IsGroupNameAvailableQueryHandler : IQueryHandler<IsGroupNameAvailableQuery, IsGroupNameAvailableQueryResult>
{
    private readonly IIsGroupNameAvailableQueryDataProvider _provider;

    public IsGroupNameAvailableQueryHandler(IIsGroupNameAvailableQueryDataProvider provider)
        => _provider = provider;

    public async Task<IsGroupNameAvailableQueryResult> HandleAsync(IsGroupNameAvailableQuery query, CancellationToken cancellationToken = default)
    {
        var trimmedName = query.Name.Trim();
        var exists = await _provider.ExistsAsync(query.CallerUserId, trimmedName, cancellationToken);
        return new IsGroupNameAvailableQueryResult(!exists);
    }
}
