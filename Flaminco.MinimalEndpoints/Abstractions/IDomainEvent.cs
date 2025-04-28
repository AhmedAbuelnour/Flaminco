namespace Flaminco.MinimalEndpoints.Abstractions
{
    using System.Threading;
    /// <summary>
    /// Represents a domain event in the application.
    /// </summary>
    public interface IDomainEvent;

    /// <summary>
    /// Defines a handler for a specific type of domain event.
    /// </summary>
    /// <typeparam name="TEvent">The type of the domain event.</typeparam>
    public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
    {
        /// <summary>
        /// Handles the specified domain event.
        /// </summary>
        /// <param name="domainEvent">The domain event to handle.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        ValueTask Handle(TEvent domainEvent, CancellationToken cancellationToken);
    }

}
