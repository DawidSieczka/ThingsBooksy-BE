namespace ThingsBooksy.Shared.Abstractions.Events.ManagementGroups;

public record GroupMemberAdded(Guid GroupId, Guid UserId) : IEvent;
