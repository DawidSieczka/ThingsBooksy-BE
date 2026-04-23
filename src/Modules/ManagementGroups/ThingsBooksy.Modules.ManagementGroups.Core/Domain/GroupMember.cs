using System;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Domain;

internal class GroupMember
{
    public Guid GroupId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime JoinedAt { get; private set; }

    public ManagementGroup Group { get; private set; } = null!;

    private GroupMember() { }

    public static GroupMember Create(Guid groupId, Guid userId, DateTime now)
        => new() { GroupId = groupId, UserId = userId, JoinedAt = now };
}
