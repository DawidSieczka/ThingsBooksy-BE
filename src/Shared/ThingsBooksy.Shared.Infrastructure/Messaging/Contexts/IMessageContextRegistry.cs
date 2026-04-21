using ThingsBooksy.Shared.Abstractions.Messaging;

namespace ThingsBooksy.Shared.Infrastructure.Messaging.Contexts;

public interface IMessageContextRegistry
{
    void Set(IMessage message, IMessageContext context);
}