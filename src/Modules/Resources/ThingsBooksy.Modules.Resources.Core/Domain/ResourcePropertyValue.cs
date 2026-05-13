using System;
using ThingsBooksy.Modules.Resources.Core.Features.CreateResourceInstance;

namespace ThingsBooksy.Modules.Resources.Core.Domain;

internal class ResourcePropertyValue
{
    public Guid Id { get; private set; }
    public Guid ResourceInstanceId { get; private set; }
    public Guid PropertyDefinitionId { get; private set; }
    public string Value { get; private set; } = null!;

    private ResourcePropertyValue() { }

    public static ResourcePropertyValue Create(PropertyValueInput input, Guid resourceInstanceId)
        => new()
        {
            Id = Guid.CreateVersion7(),
            ResourceInstanceId = resourceInstanceId,
            PropertyDefinitionId = input.PropertyDefinitionId,
            Value = input.Value
        };

    public void Update(string value)
    {
        Value = value;
    }
}
