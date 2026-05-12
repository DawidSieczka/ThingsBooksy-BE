using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThingsBooksy.Modules.Resources.Core.ReadModels;

namespace ThingsBooksy.Modules.Resources.Core.DAL.Configurations;

internal class GroupReadModelConfiguration : IEntityTypeConfiguration<GroupReadModel>
{
    public void Configure(EntityTypeBuilder<GroupReadModel> builder)
    {
        builder.ToTable("group_read_models");
        builder.HasKey(x => x.Id);
    }
}
