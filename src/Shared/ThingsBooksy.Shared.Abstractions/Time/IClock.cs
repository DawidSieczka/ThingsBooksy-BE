using System;

namespace ThingsBooksy.Shared.Abstractions.Time;

public interface IClock
{
    DateTime CurrentDate();
}