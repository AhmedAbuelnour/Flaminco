namespace Flaminco.RedisChannels.Abstractions;

/// <summary>
///     Provides a writing abstraction for a Redis Stream, similar to ChannelWriter&lt;T&gt;.
/// </summary>
/// <typeparam name="T">The type of data in the stream.</typeparam>
public interface IRedisStreamWriter<T>
{
    /// <summary>
    ///     Gets a value that indicates whether writing to the stream is possible.
    /// </summary>
    bool CanWrite { get; }

    /// <summary>
    ///     Attempts to write the specified item to the stream.
    /// </summary>
    /// <param name="item">The item to write.</param>
    /// <param name="cancellationToken">A cancellation token used to cancel the write operation.</param>
    /// <returns>A ValueTask that represents the asynchronous write operation.</returns>
    ValueTask<bool> WriteAsync(T item, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously waits for space to be available in the stream for writing.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to cancel the wait operation.</param>
    /// <returns>A ValueTask that completes when space is available or the stream is closed.</returns>
    ValueTask<bool> WaitToWriteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Marks the stream as being complete, meaning no more data will be written to it.
    /// </summary>
    /// <param name="error">Optional Exception that indicates the stream is being completed due to an error.</param>
    void Complete(Exception? error = null);
}
