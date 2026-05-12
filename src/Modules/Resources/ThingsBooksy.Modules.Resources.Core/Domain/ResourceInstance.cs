using System;
using System.Collections.Generic;

namespace ThingsBooksy.Modules.Resources.Core.Domain;

internal class ResourceInstance
{
    public Guid Id { get; private set; }
    public Guid ResourceTypeId { get; private set; }
    public Guid GroupId { get; private set; }
    public string Name { get; private set; } = null!;
    public Guid OwnerId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public string? Description { get; private set; }

    public ICollection<ResourcePropertyValue> PropertyValues { get; private set; } = new List<ResourcePropertyValue>();

    private ResourceInstance() { }

    public static ResourceInstance Create(Guid id, Guid resourceTypeId, Guid groupId, string name, string? description, Guid ownerId, DateTime now)
        => new()
        {
            Id = id,
            ResourceTypeId = resourceTypeId,
            GroupId = groupId,
            Name = name,
            Description = description,
            OwnerId = ownerId,
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
