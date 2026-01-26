namespace Flaminco.RedisChannels.Abstractions;

/// <summary>
///     Provides a reading abstraction for a Redis Stream, similar to ChannelReader&lt;T&gt;.
/// </summary>
/// <typeparam name="T">The type of data in the stream.</typeparam>
public interface IRedisStreamReader<T> : IAsyncEnumerable<T>
{
    /// <summary>
    ///     Gets a value that indicates whether reading from the stream is possible.
    /// </summary>
    bool CanRead { get; }

    /// <summary>
    ///     Gets a value that indicates whether no more data will ever be available to be read from the stream.
    /// </summary>
    bool Completion { get; }

    /// <summary>
    ///     Attempts to read an item from the stream.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to cancel the read operation.</param>
    /// <returns>A ValueTask that represents the asynchronous read operation.</returns>
    ValueTask<T?> ReadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Attempts to read an item from the stream along with its message ID for acknowledgment.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to cancel the read operation.</param>
    /// <returns>A ValueTask that represents the asynchronous read operation, containing the item and message ID.</returns>
    ValueTask<(T? Item, string MessageId)> ReadWithIdAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously waits for data to be available in the stream.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to cancel the wait operation.</param>
    /// <returns>A ValueTask that completes when data is available or the stream is closed.</returns>
    ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Acknowledges that a message has been processed and should be removed from the pending entries list.
    /// </summary>
    /// <param name="messageId">The message ID to acknowledge.</param>
    /// <param name="cancellationToken">A cancellation token used to cancel the operation.</param>
    /// <returns>A ValueTask that represents the asynchronous acknowledge operation.</returns>
    ValueTask AcknowledgeAsync(string messageId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads messages starting from a specific message ID. Useful for replaying missed messages after reconnection.
    ///     This method returns an IAsyncEnumerable that can be used with await foreach.
    /// </summary>
    /// <param name="fromMessageId">The message ID to start reading from (exclusive). Use null or empty to read from the beginning.</param>
    /// <param name="cancellationToken">A cancellation token used to cancel the operation.</param>
    /// <returns>An IAsyncEnumerable of tuples containing the item and its message ID.</returns>
    IAsyncEnumerable<(T Item, string MessageId)> ReadFromMessageIdAsync(string? fromMessageId = null, CancellationToken cancellationToken = default);
}
