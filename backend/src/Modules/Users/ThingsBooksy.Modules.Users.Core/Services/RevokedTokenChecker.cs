using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ThingsBooksy.Modules.Users.Core.DAL;
using ThingsBooksy.Shared.Abstractions.Auth;

namespace ThingsBooksy.Modules.Users.Core.Services;

internal sealed class RevokedTokenChecker : IRevokedTokenChecker
{
    private readonly UsersDbContext _dbContext;
    private readonly IMemoryCache _cache;

    public RevokedTokenChecker(UsersDbContext dbContext, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task<bool> IsRevokedAsync(string jti, CancellationToken ct = default)
    {
        // Cache only stores positive (revoked) entries — presence in the cache means revoked.
        // Negative results are never cached so that a token revoked after a previous miss
        // is detected on its next request (no 30 s blind window).
        var cacheKey = $"revoked:{jti}";
        if (_cache.TryGetValue(cacheKey, out bool _))
        {
            return true;
        }

        var revocation = await _dbContext.TokenRevocations
            .AsNoTracking()
            .Where(x => x.Jti == jti)
            .Select(x => new { x.ExpiresAt })
            .FirstOrDefaultAsync(ct);

        if (revocation is null)
        {
            return false;
        }

        var ttl = revocation.ExpiresAt - DateTime.UtcNow;
        if (ttl <= TimeSpan.Zero)
        {
            ttl = TimeSpan.FromMinutes(1);
        }

        _cache.Set(cacheKey, true, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        });

        return true;
    }
}
