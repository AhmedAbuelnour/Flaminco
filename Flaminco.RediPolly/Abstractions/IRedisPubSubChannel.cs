namespace Flaminco.RedisChannels.Abstractions;

/// <summary>
///     Provides access to the reading and writing sides of a Redis Pub/Sub channel.
///     Pub/Sub is fire-and-forget messaging - messages are not persisted and cannot be replayed.
/// </summary>
/// <typeparam name="T">The type of data in the channel.</typeparam>
public interface IRedisPubSubChannel<T>
{
    /// <summary>
    ///     Gets the readable half of the channel.
    /// </summary>
    IRedisPubSubReader<T> Reader { get; }

    /// <summary>
    ///     Gets the writable half of the channel.
    /// </summary>
    IRedisPubSubWriter<T> Writer { get; }
}
