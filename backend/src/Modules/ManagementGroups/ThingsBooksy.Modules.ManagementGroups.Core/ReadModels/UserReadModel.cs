using System;
using ThingsBooksy.Shared.Abstractions.Events.Users;

namespace ThingsBooksy.Modules.ManagementGroups.Core.ReadModels;

internal class UserReadModel
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;

    private UserReadModel() { }

    internal static UserReadModel Upsert(UserSignedUp @event)
        => new() { Id = @event.UserId, Email = @event.Email.ToLowerInvariant() };
}
