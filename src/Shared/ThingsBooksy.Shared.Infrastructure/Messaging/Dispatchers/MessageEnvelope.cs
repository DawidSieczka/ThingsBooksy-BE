using ThingsBooksy.Shared.Abstractions.Messaging;

namespace ThingsBooksy.Shared.Infrastructure.Messaging.Dispatchers;

public record MessageEnvelope(IMessage Message, IMessageContext MessageContext);