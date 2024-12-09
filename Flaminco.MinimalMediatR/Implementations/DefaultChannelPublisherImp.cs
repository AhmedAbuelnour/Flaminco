using Flaminco.MinimalMediatR.Abstractions;
using MediatR;

namespace Flaminco.MinimalMediatR.Implementations
{
    public sealed class DefaultChannelPublisherImp(InMemoryMessageQueue queue) : IChannelPublisher
    {
        public ValueTask PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
        {
            return queue.Writer.WriteAsync(notification, cancellationToken);
        }
    }
}
