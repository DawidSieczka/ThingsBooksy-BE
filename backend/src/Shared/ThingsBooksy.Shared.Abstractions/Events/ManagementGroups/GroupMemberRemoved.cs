namespace ThingsBooksy.Shared.Abstractions.Events.ManagementGroups;

public record GroupMemberRemoved(Guid GroupId, Guid UserId) : IEvent;
