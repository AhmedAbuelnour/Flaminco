namespace Flaminco.MinimalEndpoints.Abstractions
{
    /// <summary>
    /// Represents an event bus contract for publishing and consuming domain events.
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Publishes a domain event.
        /// </summary>
        /// <typeparam name="TDomainEvent">The concrete event type.</typeparam>
        /// <param name="domainEvent">The event instance.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        ValueTask Publish<TDomainEvent>(TDomainEvent domainEvent, CancellationToken cancellationToken = default)
            where TDomainEvent : IDomainEvent;

        /// <summary>
        /// Reads all published domain events as an async stream.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>An async stream of domain events.</returns>
        IAsyncEnumerable<IDomainEvent> ReadAllAsync(CancellationToken cancellationToken = default);
    }
}

