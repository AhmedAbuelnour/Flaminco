using StackExchange.Redis;

namespace LowCodeHub.Chat.SSE.Options;

/// <summary>
/// Production-oriented options for Redis Streams + native ASP.NET Core SSE chat.
/// </summary>
public sealed class RedisChatSseOptions
{
    /// <summary>
    /// Redis connection string (for example, localhost:6379).
    /// Ignored if <see cref="ConnectionMultiplexer"/> is provided.
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Optional external multiplexer. If provided, package will not own/dispose it.
    /// </summary>
    public IConnectionMultiplexer? ConnectionMultiplexer { get; set; }

    /// <summary>
    /// Prefix for stream keys. Final key format: {prefix}{channelName}
    /// </summary>
    public string StreamKeyPrefix { get; set; } = "chat:sse:";

    /// <summary>
    /// Maximum number of stream entries to retain.
    /// </summary>
    public long MaxStreamLength { get; set; } = 50_000;

    /// <summary>
    /// Approximate stream trimming behavior.
    /// </summary>
    public bool UseApproximateTrimming { get; set; } = true;

    /// <summary>
    /// Poll interval in milliseconds when no new stream messages are found.
    /// </summary>
    public int PollIntervalMs { get; set; } = 300;

    /// <summary>
    /// Maximum entries fetched in one polling cycle.
    /// </summary>
    public int ReadBatchSize { get; set; } = 50;

    /// <summary>
    /// Maximum entries replayed when Last-Event-ID is provided.
    /// </summary>
    public int ReplayBatchSize { get; set; } = 200;

    /// <summary>
    /// Heartbeat interval in seconds for SSE keep-alive messages.
    /// </summary>
    public int HeartbeatIntervalSeconds { get; set; } = 15;

    /// <summary>
    /// SSE event type used for live chat messages.
    /// </summary>
    public string ChatMessageEventType { get; set; } = "chat-message";

    /// <summary>
    /// Maximum UTF-8 payload size in bytes to publish.
    /// </summary>
    public int MaxPayloadBytes { get; set; } = 16 * 1024;

    /// <summary>
    /// Whether to require authenticated users for endpoints mapped by this package.
    /// </summary>
    public bool RequireAuthenticatedUser { get; set; }
}

