using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThingsBooksy.Modules.Users.Core.Entities;

namespace ThingsBooksy.Modules.Users.Core.DAL.Configurations;

internal class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(x => x.Name);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder
            .Property(x => x.Permissions)
            .HasConversion(
                x => string.Join(',', x),
                x => x.Split(',', StringSplitOptions.RemoveEmptyEntries));

        builder.Property(x => x.Permissions).Metadata.SetValueComparer(
            new ValueComparer<IEnumerable<string>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Any() ? c.Aggregate(0, (a, next) => HashCode.Combine(a, next.GetHashCode())) : 0));
    }
}
