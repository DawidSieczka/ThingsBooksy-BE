using System;
using ThingsBooksy.Shared.Abstractions.Contexts;

namespace ThingsBooksy.Shared.Abstractions.Messaging;

public interface IMessageContext
{
    public Guid MessageId { get; }
    public IContext Context { get; }
}