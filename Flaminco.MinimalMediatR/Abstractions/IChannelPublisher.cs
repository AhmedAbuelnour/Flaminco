using MediatR;

namespace Flaminco.MinimalMediatR.Abstractions
{
    public interface IChannelPublisher
    {
        ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification;
    }
}
