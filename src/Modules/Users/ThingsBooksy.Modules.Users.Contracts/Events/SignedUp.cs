using System;
using ThingsBooksy.Shared.Abstractions.Events;

namespace ThingsBooksy.Modules.Users.Contracts.Events;

public record SignedUp(Guid UserId, string Email, string Role, string? JobTitle) : IEvent;
