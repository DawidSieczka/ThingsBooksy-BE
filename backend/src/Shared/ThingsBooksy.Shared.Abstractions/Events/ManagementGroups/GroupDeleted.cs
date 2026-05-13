namespace ThingsBooksy.Shared.Abstractions.Events.ManagementGroups;

public record GroupDeleted(Guid GroupId) : IEvent;
