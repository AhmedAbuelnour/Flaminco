namespace Flaminco.RabbitMQ.AMQP.Configuration;

/// <summary>
/// Represents a queue declaration configuration.
/// </summary>
public sealed class QueueDeclaration
{
    /// <summary>
    /// Gets or sets the queue name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets whether the queue survives broker restarts.
    /// </summary>
    public bool Durable { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the queue is exclusive to the connection.
    /// </summary>
    public bool Exclusive { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the queue is deleted when the last consumer unsubscribes.
    /// </summary>
    public bool AutoDelete { get; set; } = false;

    /// <summary>
    /// Gets or sets additional queue arguments (e.g., x-message-ttl, x-max-length).
    /// </summary>
    public Dictionary<string, object?> Arguments { get; set; } = [];

    /// <summary>
    /// Gets or sets the dead-letter exchange for rejected messages.
    /// </summary>
    public string? DeadLetterExchange { get; set; }

    /// <summary>
    /// Gets or sets the dead-letter routing key.
    /// </summary>
    public string? DeadLetterRoutingKey { get; set; }

    /// <summary>
    /// Gets or sets the message TTL in milliseconds.
    /// </summary>
    public int? MessageTtl { get; set; }

    /// <summary>
    /// Gets or sets the maximum queue length.
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets the maximum queue size in bytes.
    /// </summary>
    public long? MaxLengthBytes { get; set; }

    /// <summary>
    /// Builds the queue arguments dictionary including dead-letter and TTL settings.
    /// </summary>
    public Dictionary<string, object?> BuildArguments()
    {
        var args = new Dictionary<string, object?>(Arguments);

        if (!string.IsNullOrEmpty(DeadLetterExchange))
            args["x-dead-letter-exchange"] = DeadLetterExchange;

        if (!string.IsNullOrEmpty(DeadLetterRoutingKey))
            args["x-dead-letter-routing-key"] = DeadLetterRoutingKey;

        if (MessageTtl.HasValue)
            args["x-message-ttl"] = MessageTtl.Value;

        if (MaxLength.HasValue)
            args["x-max-length"] = MaxLength.Value;

        if (MaxLengthBytes.HasValue)
            args["x-max-length-bytes"] = MaxLengthBytes.Value;

        return args;
    }
}
