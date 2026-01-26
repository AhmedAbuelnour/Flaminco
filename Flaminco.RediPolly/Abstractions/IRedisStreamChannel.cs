namespace Flaminco.RedisChannels.Abstractions;

/// <summary>
///     Provides access to the reading and writing sides of a Redis Stream channel.
/// </summary>
/// <typeparam name="T">The type of data in the channel.</typeparam>
public interface IRedisStreamChannel<T>
{
    /// <summary>
    ///     Gets the readable half of the channel.
    /// </summary>
    IRedisStreamReader<T> Reader { get; }

    /// <summary>
    ///     Gets the writable half of the channel.
    /// </summary>
    IRedisStreamWriter<T> Writer { get; }
}
