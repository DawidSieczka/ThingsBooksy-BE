using ThingsBooksy.Modules.Users.Core.Features.GetUser.DataProviders;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.Users.Core.Features.GetUser;

internal sealed class GetUserQueryHandler : IQueryHandler<GetUserQuery, GetUserQueryResult?>
{
    private readonly IGetUserQueryDataProvider _provider;

    public GetUserQueryHandler(IGetUserQueryDataProvider provider)
        => _provider = provider;

    public async Task<GetUserQueryResult?> HandleAsync(GetUserQuery query, CancellationToken cancellationToken = default)
    {
        var user = await _provider.GetByIdAsync(query.UserId, cancellationToken);
        if (user is null) return null;

        return new GetUserQueryResult(
            user.Id,
            user.Email,
            user.Role.Name,
            user.JobTitle,
            user.CreatedAt,
            user.Role.Permissions);
    }
}
