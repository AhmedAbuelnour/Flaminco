namespace Flaminco.RedisChannels.Abstractions;

/// <summary>
///     Provides a writing abstraction for a Redis Pub/Sub channel, similar to ChannelWriter&lt;T&gt;.
///     Pub/Sub is fire-and-forget messaging - messages are not persisted.
/// </summary>
/// <typeparam name="T">The type of data in the channel.</typeparam>
public interface IRedisPubSubWriter<T>
{
    /// <summary>
    ///     Gets a value that indicates whether writing to the channel is possible.
    /// </summary>
    bool CanWrite { get; }

    /// <summary>
    ///     Attempts to write the specified item to the channel.
    /// </summary>
    /// <param name="item">The item to write.</param>
    /// <param name="cancellationToken">A cancellation token used to cancel the write operation.</param>
    /// <returns>A ValueTask that represents the asynchronous write operation.</returns>
    ValueTask<bool> WriteAsync(T item, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously waits for space to be available in the channel for writing.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to cancel the wait operation.</param>
    /// <returns>A ValueTask that completes when space is available or the channel is closed.</returns>
    ValueTask<bool> WaitToWriteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Marks the channel as being complete, meaning no more data will be written to it.
    /// </summary>
    /// <param name="error">Optional Exception that indicates the channel is being completed due to an error.</param>
    void Complete(Exception? error = null);
}
