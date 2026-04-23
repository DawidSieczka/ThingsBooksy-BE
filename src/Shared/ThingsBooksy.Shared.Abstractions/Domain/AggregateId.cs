using System;
using ThingsBooksy.Shared.Abstractions.Exceptions;

namespace ThingsBooksy.Shared.Abstractions.Domain;

public record AggregateId
{
    public Guid Value { get; }

    public AggregateId() : this(Guid.CreateVersion7())
    {
    }

    public AggregateId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new InvalidAggregateIdException(value);
        }

        Value = value;
    }

    public static AggregateId Create()
        => new(Guid.CreateVersion7());

    public static implicit operator Guid(AggregateId id)
        => id.Value;

    public static implicit operator AggregateId(Guid id)
        => new(id);

    public override string ToString() => Value.ToString();
}