namespace LowCodeHub.Chat.SSE.Contracts;

/// <summary>
/// Base message contract required by Redis Streams + SSE pipeline.
/// </summary>
public interface IChatStreamMessage
{
    /// <summary>
    /// Domain-level message identifier.
    /// </summary>
    string MessageId { get; }

    /// <summary>
    /// Logical chat channel.
    /// </summary>
    string Channel { get; }

    /// <summary>
    /// Sender identifier.
    /// </summary>
    string SenderId { get; }

    /// <summary>
    /// Message content.
    /// </summary>
    string Content { get; }

    /// <summary>
    /// UTC send time.
    /// </summary>
    DateTimeOffset SentAtUtc { get; }

    /// <summary>
    /// Optional metadata bag.
    /// </summary>
    IReadOnlyDictionary<string, string>? Metadata { get; }
}

