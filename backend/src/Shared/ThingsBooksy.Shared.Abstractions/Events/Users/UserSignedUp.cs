using System;

namespace ThingsBooksy.Shared.Abstractions.Events.Users;

public record UserSignedUp(Guid UserId, string Email) : IEvent;
