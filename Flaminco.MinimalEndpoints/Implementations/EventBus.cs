using Flaminco.MinimalEndpoints.Abstractions;
using System.Threading.Channels;

namespace Flaminco.MinimalEndpoints.Implementations
{
    public sealed class EventBus
    {
        private readonly Channel<IDomainEvent> _channel = Channel.CreateUnbounded<IDomainEvent>();

        public ValueTask Publish<TDomainEvent>(TDomainEvent domainEvent, CancellationToken cancellationToken = default) where TDomainEvent : IDomainEvent
        {
            return Writer.WriteAsync(domainEvent, cancellationToken);
        }

        internal ChannelWriter<IDomainEvent> Writer => _channel.Writer;
        internal ChannelReader<IDomainEvent> Reader => _channel.Reader;
    }
}
