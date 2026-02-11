namespace LowCodeHub.Chat.SSE.Contracts;

/// <summary>
/// Represents one replay entry returned from an external history source (DB, API, archive, etc.).
/// </summary>
/// <typeparam name="TMessage">Caller-defined message type.</typeparam>
public sealed record ChatHistoryEntry<TMessage>(
    string EventId,
    TMessage Message,
    string EventType = "chat-message")
    where TMessage : class, IChatStreamMessage;

