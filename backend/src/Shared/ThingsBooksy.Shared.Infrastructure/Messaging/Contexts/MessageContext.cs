using System;
using ThingsBooksy.Shared.Abstractions.Contexts;
using ThingsBooksy.Shared.Abstractions.Messaging;

namespace ThingsBooksy.Shared.Infrastructure.Messaging.Contexts;

public class MessageContext : IMessageContext
{
    public Guid MessageId { get; }
    public IContext Context { get; }

    public MessageContext(Guid messageId, IContext context)
    {
        MessageId = messageId;
        Context = context;
    }
}