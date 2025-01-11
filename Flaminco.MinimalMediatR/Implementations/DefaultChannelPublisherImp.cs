using Flaminco.MinimalMediatR.Abstractions;
using MediatR;

namespace Flaminco.MinimalMediatR.Implementations
{
    internal sealed class DefaultChannelPublisherImp(InMemoryMessageQueue queue) : IChannelPublisher
    {
        public ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
        {
            return queue.Writer.WriteAsync(notification, cancellationToken);
        }
    }
}
