using System;
using System.Collections.Generic;

namespace ThingsBooksy.Modules.Resources.Core.Domain;

internal class ResourceType
{
    public Guid Id { get; private set; }
    public Guid GroupId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public ICollection<ResourcePropertyDefinition> PropertyDefinitions { get; private set; } = new List<ResourcePropertyDefinition>();

    private ResourceType() { }

    public static ResourceType Create(Guid id, Guid groupId, string name, string? description, DateTime now)
        => new()
        {
            Id = id,
            GroupId = groupId,
            Name = name,
            Description = description,
            CreatedAt = now,
            UpdatedAt = now
        };

    public void Update(string name, string? description, DateTime now)
    {
        Name = name;
        Description = description;
        UpdatedAt = now;
    }

    public void Delete(DateTime now)
    {
        DeletedAt = now;
        UpdatedAt = now;
    }

    public bool IsDeleted => DeletedAt.HasValue;
}
