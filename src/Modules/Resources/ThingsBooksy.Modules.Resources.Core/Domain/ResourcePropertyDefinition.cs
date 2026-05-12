using System;

namespace ThingsBooksy.Modules.Resources.Core.Domain;

internal class ResourcePropertyDefinition
{
    public Guid Id { get; private set; }
    public Guid ResourceTypeId { get; private set; }
    public string Name { get; private set; } = null!;
    public PropertyDataType DataType { get; private set; }
    public bool IsRequired { get; private set; }

    private ResourcePropertyDefinition() { }

    public static ResourcePropertyDefinition Create(Guid id, Guid resourceTypeId, string name, PropertyDataType dataType, bool isRequired)
        => new()
        {
            Id = id,
            ResourceTypeId = resourceTypeId,
            Name = name,
            DataType = dataType,
            IsRequired = isRequired
        };

    public void Update(string name, PropertyDataType dataType, bool isRequired)
    {
        Name = name;
        DataType = dataType;
        IsRequired = isRequired;
    }
}
