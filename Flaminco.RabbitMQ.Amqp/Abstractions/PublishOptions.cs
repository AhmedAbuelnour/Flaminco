namespace Flaminco.RabbitMQ.AMQP.Abstractions;

/// <summary>
/// Options for message publishing.
/// </summary>
public sealed class PublishOptions
{
    /// <summary>
    /// Gets or sets the message ID. Auto-generated if not provided.
    /// </summary>
    public string? MessageId { get; set; }

    /// <summary>
    /// Gets or sets the correlation ID for request-reply patterns.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the reply-to queue name for RPC patterns.
    /// </summary>
    public string? ReplyTo { get; set; }

    /// <summary>
    /// Gets or sets whether the message is persistent (survives broker restart). Default is true.
    /// </summary>
    public bool Persistent { get; set; } = true;

    /// <summary>
    /// Gets or sets the message priority (0-9).
    /// </summary>
    public byte? Priority { get; set; }

    /// <summary>
    /// Gets or sets the message expiration in milliseconds.
    /// </summary>
    public string? Expiration { get; set; }

    /// <summary>
    /// Gets or sets the message type identifier.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the user ID (for authentication).
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Gets or sets the application ID.
    /// </summary>
    public string? AppId { get; set; }

    /// <summary>
    /// Gets or sets custom headers.
    /// </summary>
    public Dictionary<string, object?> Headers { get; set; } = [];

    /// <summary>
    /// Gets or sets whether to use mandatory publishing (returns unroutable messages). Default is false.
    /// </summary>
    public bool Mandatory { get; set; } = false;

    /// <summary>
    /// Gets or sets the timeout for publisher confirms in milliseconds. Default is 30000.
    /// </summary>
    public int ConfirmTimeoutMs { get; set; } = 30000;

    /// <summary>
    /// Creates publish options with a correlation ID.
    /// </summary>
    /// <param name="correlationId">The correlation ID.</param>
    /// <returns>A new PublishOptions instance.</returns>
    public static PublishOptions WithCorrelationId(string correlationId) => new() { CorrelationId = correlationId };

    /// <summary>
    /// Creates publish options for RPC patterns.
    /// </summary>
    /// <param name="replyTo">The reply-to queue.</param>
    /// <param name="correlationId">The correlation ID.</param>
    /// <returns>A new PublishOptions instance.</returns>
    public static PublishOptions ForRpc(string replyTo, string? correlationId = null) => new()
    {
        ReplyTo = replyTo,
        CorrelationId = correlationId ?? Guid.NewGuid().ToString()
    };

    /// <summary>
    /// Creates publish options with expiration.
    /// </summary>
    /// <param name="expirationMs">The expiration in milliseconds.</param>
    /// <returns>A new PublishOptions instance.</returns>
    public static PublishOptions WithExpiration(int expirationMs) => new() { Expiration = expirationMs.ToString() };
}
