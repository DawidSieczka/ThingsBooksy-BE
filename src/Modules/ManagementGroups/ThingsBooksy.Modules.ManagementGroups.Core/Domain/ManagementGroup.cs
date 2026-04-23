using System;
using System.Collections.Generic;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Domain;

internal class ManagementGroup
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid OwnerId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public ICollection<GroupMember> Members { get; private set; } = new List<GroupMember>();

    private ManagementGroup() { }

    public static ManagementGroup Create(Guid id, string name, string? description, Guid ownerId, DateTime now)
        => new()
        {
            Id = id,
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

    public void Restore(DateTime now)
    {
        DeletedAt = null;
        UpdatedAt = now;
    }

    public bool IsDeleted => DeletedAt.HasValue;
}
