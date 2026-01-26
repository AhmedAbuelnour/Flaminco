namespace Flaminco.RedisChannels.Abstractions;

/// <summary>
///     Provides a reading abstraction for a Redis Pub/Sub channel, similar to ChannelReader&lt;T&gt;.
///     Pub/Sub is fire-and-forget messaging - messages are not persisted and cannot be replayed.
/// </summary>
/// <typeparam name="T">The type of data in the channel.</typeparam>
public interface IRedisPubSubReader<T> : IAsyncEnumerable<T>
{
    /// <summary>
    ///     Gets a value that indicates whether reading from the channel is possible.
    /// </summary>
    bool CanRead { get; }

    /// <summary>
    ///     Gets a value that indicates whether no more data will ever be available to be read from the channel.
    /// </summary>
    bool Completion { get; }

    /// <summary>
    ///     Attempts to read an item from the channel.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to cancel the read operation.</param>
    /// <returns>A ValueTask that represents the asynchronous read operation.</returns>
    ValueTask<T?> ReadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously waits for data to be available in the channel.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to cancel the wait operation.</param>
    /// <returns>A ValueTask that completes when data is available or the channel is closed.</returns>
    ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default);
}
