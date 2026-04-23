using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThingsBooksy.Modules.ManagementGroups.Core.Domain;

namespace ThingsBooksy.Modules.ManagementGroups.Core.DAL.Configurations;

internal class GroupMemberConfiguration : IEntityTypeConfiguration<GroupMember>
{
    public void Configure(EntityTypeBuilder<GroupMember> builder)
    {
        builder.HasKey(x => new { x.GroupId, x.UserId });
    }
}
