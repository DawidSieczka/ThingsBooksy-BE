using System;

namespace ThingsBooksy.Modules.Resources.Core.Domain;

internal class ResourcePropertyValue
{
    public Guid Id { get; private set; }
    public Guid ResourceInstanceId { get; private set; }
    public Guid PropertyDefinitionId { get; private set; }
    public string Value { get; private set; } = null!;

    private ResourcePropertyValue() { }

    public static ResourcePropertyValue Create(Guid id, Guid resourceInstanceId, Guid propertyDefinitionId, string value)
        => new()
        {
            Id = id,
            ResourceInstanceId = resourceInstanceId,
            PropertyDefinitionId = propertyDefinitionId,
            Value = value
        };

    public void Update(string value)
    {
        Value = value;
    }
}
