using System;
using ThingsBooksy.Modules.Resources.Core.Features.CreateResourceType;
using ThingsBooksy.Modules.Resources.Core.Features.UpdateResourceType;

namespace ThingsBooksy.Modules.Resources.Core.Domain;

internal class ResourcePropertyDefinition
{
    public Guid Id { get; private set; }
    public Guid ResourceTypeId { get; private set; }
    public string Name { get; private set; } = null!;
    public PropertyDataType DataType { get; private set; }
    public bool IsRequired { get; private set; }

    private ResourcePropertyDefinition() { }

    public static ResourcePropertyDefinition Create(PropertyDefinitionInput input, Guid resourceTypeId)
        => new()
        {
            Id = Guid.CreateVersion7(),
            ResourceTypeId = resourceTypeId,
            Name = input.Name,
            DataType = input.DataType,
            IsRequired = input.IsRequired
        };

    public void Update(PropertyDefinitionUpdateInput input)
    {
        Name = input.Name;
        DataType = input.DataType;
        IsRequired = input.IsRequired;
    }
}
