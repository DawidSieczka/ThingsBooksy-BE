using System;
using ThingsBooksy.Shared.Abstractions.Events.ManagementGroups;

namespace ThingsBooksy.Modules.Resources.Core.ReadModels;

internal class GroupMemberReadModel
{
    public Guid GroupId { get; private set; }
    public Guid UserId { get; private set; }

    private GroupMemberReadModel() { }

    internal static GroupMemberReadModel Upsert(GroupMemberAdded @event)
        => new() { GroupId = @event.GroupId, UserId = @event.UserId };
}
