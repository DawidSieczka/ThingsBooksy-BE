using System;

namespace ThingsBooksy.Shared.Abstractions.Events;

public record UserSignedUp(Guid UserId, string Email) : IEvent;
