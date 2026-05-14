using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.Users.Core.DAL;
using ThingsBooksy.Modules.Users.Core.Entities;

namespace ThingsBooksy.Modules.Users.Core.Features.Logout.DataProviders;

internal sealed class LogoutCommandDataProvider : ILogoutCommandDataProvider
{
    private readonly UsersDbContext _dbContext;

    public LogoutCommandDataProvider(UsersDbContext dbContext)
        => _dbContext = dbContext;

    public async Task RevokeAsync(string jti, Guid userId, DateTime expiresAt, DateTime now, CancellationToken ct = default)
    {
        // Idempotent: a second logout call for the same token is a no-op.
        // Without this, the unique index on Jti would throw DbUpdateException → 500.
        var alreadyRevoked = await _dbContext.TokenRevocations
            .AsNoTracking()
            .AnyAsync(x => x.Jti == jti, ct);
        if (alreadyRevoked)
        {
            return;
        }

        var revocation = TokenRevocation.Create(jti, userId, expiresAt, now);
        _dbContext.TokenRevocations.Add(revocation);
        await _dbContext.SaveChangesAsync(ct);
    }
}
