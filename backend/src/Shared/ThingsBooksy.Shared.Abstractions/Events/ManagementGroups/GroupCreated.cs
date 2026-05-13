namespace ThingsBooksy.Shared.Abstractions.Events.ManagementGroups;

public record GroupCreated(Guid GroupId, Guid OwnerId) : IEvent;
