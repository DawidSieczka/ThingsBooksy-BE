using System;
using ThingsBooksy.Shared.Abstractions.Events;

namespace ThingsBooksy.Modules.Users.Contracts.Events;

public record SignedIn(Guid UserId) : IEvent;
