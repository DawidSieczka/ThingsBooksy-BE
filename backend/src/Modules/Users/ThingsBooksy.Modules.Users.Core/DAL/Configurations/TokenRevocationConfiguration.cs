using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThingsBooksy.Modules.Users.Core.Entities;

namespace ThingsBooksy.Modules.Users.Core.DAL.Configurations;

internal class TokenRevocationConfiguration : IEntityTypeConfiguration<TokenRevocation>
{
    public void Configure(EntityTypeBuilder<TokenRevocation> builder)
    {
        builder.ToTable("token_revocations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Jti).IsRequired().HasMaxLength(128);
        builder.HasIndex(x => x.Jti).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.RevokedAt });
    }
}
