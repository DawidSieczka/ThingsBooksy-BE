using System;
using ThingsBooksy.Shared.Abstractions.Time;

namespace ThingsBooksy.Shared.Infrastructure.Time;

public class UtcClock : IClock
{
    public DateTime CurrentDate() => DateTime.UtcNow;
}