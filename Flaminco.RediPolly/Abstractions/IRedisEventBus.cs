namespace Flaminco.RedisChannels.Abstractions;

/// <summary>
///     Represents a Redis-backed event bus abstraction designed to mimic the in-memory Channel-based event bus pattern.
/// </summary>
/// <typeparam name="TEventBase">The base contract for all events (for example, IDomainEvent).</typeparam>
public interface IRedisEventBus<TEventBase>
{
    /// <summary>
    ///     Publishes an event to Redis.
    /// </summary>
    /// <typeparam name="TEvent">The concrete event type.</typeparam>
    /// <param name="eventItem">The event instance to publish.</param>
    /// <param name="cancellationToken">A token to cancel the publish operation.</param>
    /// <returns>A value indicating whether publishing succeeded.</returns>
    ValueTask<bool> Publish<TEvent>(TEvent eventItem, CancellationToken cancellationToken = default)
        where TEvent : TEventBase;

    /// <summary>
    ///     Gets the readable side of the bus.
    /// </summary>
    IRedisStreamReader<TEventBase> Reader { get; }

    /// <summary>
    ///     Gets the writable side of the bus.
    /// </summary>
    IRedisStreamWriter<TEventBase> Writer { get; }
}
