using MediatR;

namespace Flaminco.MinimalMediatR.Abstractions
{
    public interface IChannelPublisher
    {
        ValueTask PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification;
    }
}
