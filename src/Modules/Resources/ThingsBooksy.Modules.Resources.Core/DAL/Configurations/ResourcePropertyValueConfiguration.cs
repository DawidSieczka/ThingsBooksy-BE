using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThingsBooksy.Modules.Resources.Core.Domain;

namespace ThingsBooksy.Modules.Resources.Core.DAL.Configurations;

internal class ResourcePropertyValueConfiguration : IEntityTypeConfiguration<ResourcePropertyValue>
{
    public void Configure(EntityTypeBuilder<ResourcePropertyValue> builder)
    {
        builder.ToTable("resource_property_values");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Value).IsRequired();
    }
}
