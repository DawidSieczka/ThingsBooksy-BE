using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Shared.Abstractions.Messaging;

namespace ThingsBooksy.Shared.Infrastructure.Messaging.Dispatchers;

public interface IAsyncMessageDispatcher
{
    Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class, IMessage;
}