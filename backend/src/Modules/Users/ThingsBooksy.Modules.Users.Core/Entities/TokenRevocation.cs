namespace ThingsBooksy.Modules.Users.Core.Entities;

internal class TokenRevocation
{
    public Guid Id { get; private set; }
    public string Jti { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public DateTime RevokedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    private TokenRevocation() { }

    public static TokenRevocation Create(string jti, Guid userId, DateTime expiresAt, DateTime now)
        => new()
        {
            Id = Guid.CreateVersion7(),
            Jti = jti,
            UserId = userId,
            ExpiresAt = expiresAt,
            RevokedAt = now
        };
}
