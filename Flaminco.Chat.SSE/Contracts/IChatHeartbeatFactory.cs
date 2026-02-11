namespace LowCodeHub.Chat.SSE.Contracts;

/// <summary>
/// Caller-implemented heartbeat factory used by SSE stream for keep-alive events.
/// </summary>
/// <typeparam name="TMessage">Caller message model.</typeparam>
public interface IChatHeartbeatFactory<TMessage>
    where TMessage : class, IChatStreamMessage
{
    /// <summary>
    /// Creates a heartbeat message for the given channel.
    /// </summary>
    TMessage Create(string channel);
}

