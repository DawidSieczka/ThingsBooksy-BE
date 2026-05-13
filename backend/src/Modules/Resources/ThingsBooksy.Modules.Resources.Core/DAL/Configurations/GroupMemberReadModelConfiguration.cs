using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThingsBooksy.Modules.Resources.Core.ReadModels;

namespace ThingsBooksy.Modules.Resources.Core.DAL.Configurations;

internal class GroupMemberReadModelConfiguration : IEntityTypeConfiguration<GroupMemberReadModel>
{
    public void Configure(EntityTypeBuilder<GroupMemberReadModel> builder)
    {
        builder.ToTable("group_member_read_models");
        builder.HasKey(x => new { x.GroupId, x.UserId });
    }
}
