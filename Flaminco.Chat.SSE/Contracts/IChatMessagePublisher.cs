namespace LowCodeHub.Chat.SSE.Contracts;

/// <summary>
/// Publishes chat messages to Redis Streams.
/// </summary>
public interface IChatMessagePublisher
{
    /// <summary>
    /// Publishes a chat message to the target channel.
    /// </summary>
    /// <returns>Redis stream entry ID.</returns>
    ValueTask<string> PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class, IChatStreamMessage;
}

