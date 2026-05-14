using ThingsBooksy.Shared.Abstractions.DataProviders;

namespace ThingsBooksy.Modules.Users.Core.Features.Logout.DataProviders;

internal interface ILogoutCommandDataProvider : IDataProvider
{
    Task RevokeAsync(string jti, Guid userId, DateTime expiresAt, DateTime now, CancellationToken ct = default);
}
