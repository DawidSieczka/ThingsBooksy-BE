using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThingsBooksy.Modules.Users.Core.Entities;

namespace ThingsBooksy.Modules.Users.Core.DAL.Configurations;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(x => x.Email).IsUnique();
        builder.Property(x => x.Email).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Password).IsRequired().HasMaxLength(500);
        builder.Property(x => x.JobTitle).HasMaxLength(200);
        builder.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId);
    }
}
