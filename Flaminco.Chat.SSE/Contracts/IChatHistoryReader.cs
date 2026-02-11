namespace LowCodeHub.Chat.SSE.Contracts;

/// <summary>
/// Caller-implemented history reader used to replay missed events from DB/API/archive.
/// </summary>
/// <typeparam name="TMessage">Caller message model.</typeparam>
public interface IChatHistoryReader<TMessage> where TMessage : class, IChatStreamMessage
{
    /// <summary>
    /// Returns history entries after the provided last event id for the given channel.
    /// </summary>
    IAsyncEnumerable<ChatHistoryEntry<TMessage>> ReadAfterAsync(string channel, string? lastEventId, CancellationToken cancellationToken = default);
}

