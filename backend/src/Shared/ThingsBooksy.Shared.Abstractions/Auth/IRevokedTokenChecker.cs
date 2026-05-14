namespace ThingsBooksy.Shared.Abstractions.Auth;

public interface IRevokedTokenChecker
{
    Task<bool> IsRevokedAsync(string jti, CancellationToken ct = default);
}
