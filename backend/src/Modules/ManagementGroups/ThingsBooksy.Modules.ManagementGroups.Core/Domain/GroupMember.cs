using System;
using ThingsBooksy.Modules.ManagementGroups.Core.Features.AddGroupMember;

namespace ThingsBooksy.Modules.ManagementGroups.Core.Domain;

internal class GroupMember
{
    public Guid GroupId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime JoinedAt { get; private set; }

    private GroupMember() { }

    public static GroupMember Create(AddGroupMemberCommand command, Guid userId, DateTime now)
        => new() { GroupId = command.GroupId, UserId = userId, JoinedAt = now };

    public ManagementGroup Group { get; private set; } = null!;
}
