using Flaminco.MinimalEndpoints.Abstractions;
using System.Threading.Channels;

namespace Flaminco.MinimalEndpoints.Implementations
{
    /// <summary>
    /// Represents an event bus for publishing and subscribing to domain events.
    /// </summary>
    public sealed class EventBus
    {
        private readonly Channel<IDomainEvent> _channel = Channel.CreateUnbounded<IDomainEvent>();

        /// <summary>
        /// Publishes a domain event to the event bus.
        /// </summary>
        /// <typeparam name="TDomainEvent">The type of the domain event.</typeparam>
        /// <param name="domainEvent">The domain event to publish.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        public ValueTask Publish<TDomainEvent>(TDomainEvent domainEvent, CancellationToken cancellationToken = default) where TDomainEvent : IDomainEvent
        {
            return Writer.WriteAsync(domainEvent, cancellationToken);
        }

        /// <summary>
        /// Gets the writer for the event bus channel.
        /// </summary>
        internal ChannelWriter<IDomainEvent> Writer => _channel.Writer;

        /// <summary>
        /// Gets the reader for the event bus channel.
        /// </summary>
        internal ChannelReader<IDomainEvent> Reader => _channel.Reader;
    }
}
