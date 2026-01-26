namespace Flaminco.RabbitMQ.AMQP.Configuration;

/// <summary>
/// Configuration options for RabbitMQ topology that can be loaded from appsettings.json.
/// Provides a hierarchical structure where exchanges contain their queues and bindings.
/// </summary>
public sealed class TopologyOptions
{
    /// <summary>
    /// The configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "RabbitMQ:Topology";

    /// <summary>
    /// Gets or sets the list of exchange configurations with their associated queues.
    /// </summary>
    public List<ExchangeTopology> Exchanges { get; set; } = [];

    /// <summary>
    /// Gets or sets standalone queues that use the default exchange (direct queue publishing).
    /// </summary>
    public List<QueueTopology> Queues { get; set; } = [];
}

/// <summary>
/// Exchange configuration with nested queues and bindings.
/// </summary>
public sealed class ExchangeTopology
{
    /// <summary>
    /// Gets or sets the exchange name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the exchange type (direct, fanout, topic, headers). Default is "direct".
    /// </summary>
    public string Type { get; set; } = ExchangeTypes.Direct;

    /// <summary>
    /// Gets or sets whether the exchange is durable. Default is true.
    /// </summary>
    public bool Durable { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the exchange is auto-deleted. Default is false.
    /// </summary>
    public bool AutoDelete { get; set; } = false;

    /// <summary>
    /// Gets or sets the queues bound to this exchange.
    /// </summary>
    public List<BoundQueueTopology> Queues { get; set; } = [];
}

/// <summary>
/// Queue configuration that is bound to an exchange.
/// </summary>
public sealed class BoundQueueTopology
{
    /// <summary>
    /// Gets or sets the queue name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the routing key for binding to the parent exchange.
    /// For topic exchanges, supports wildcards (* and #).
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the queue is durable. Default is true.
    /// </summary>
    public bool Durable { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the queue is exclusive. Default is false.
    /// </summary>
    public bool Exclusive { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the queue is auto-deleted. Default is false.
    /// </summary>
    public bool AutoDelete { get; set; } = false;

    /// <summary>
    /// Gets or sets the message TTL in milliseconds.
    /// </summary>
    public int? MessageTtl { get; set; }

    /// <summary>
    /// Gets or sets the maximum queue length (number of messages).
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets the maximum queue size in bytes.
    /// </summary>
    public long? MaxLengthBytes { get; set; }

    /// <summary>
    /// Gets or sets the maximum priority (0-255). Enables priority queue when set.
    /// </summary>
    public int? MaxPriority { get; set; }

    /// <summary>
    /// Gets or sets the queue mode ("lazy" for lazy queues).
    /// </summary>
    public string? QueueMode { get; set; }

    /// <summary>
    /// Gets or sets the dead-letter configuration. When set, automatically creates the DLX exchange and DLQ queue.
    /// </summary>
    public DeadLetterTopology? DeadLetter { get; set; }

    /// <summary>
    /// Gets or sets the prefetch count for consumers of this queue. Uses global default if not set.
    /// </summary>
    public ushort? PrefetchCount { get; set; }

    /// <summary>
    /// Builds the queue arguments dictionary.
    /// </summary>
    internal Dictionary<string, object?> BuildArguments()
    {
        var args = new Dictionary<string, object?>();

        if (DeadLetter is not null)
        {
            args["x-dead-letter-exchange"] = DeadLetter.Exchange;
            if (!string.IsNullOrEmpty(DeadLetter.RoutingKey))
                args["x-dead-letter-routing-key"] = DeadLetter.RoutingKey;
        }

        if (MessageTtl.HasValue)
            args["x-message-ttl"] = MessageTtl.Value;

        if (MaxLength.HasValue)
            args["x-max-length"] = MaxLength.Value;

        if (MaxLengthBytes.HasValue)
            args["x-max-length-bytes"] = MaxLengthBytes.Value;

        if (MaxPriority.HasValue)
            args["x-max-priority"] = MaxPriority.Value;

        if (!string.IsNullOrEmpty(QueueMode))
            args["x-queue-mode"] = QueueMode;

        return args;
    }
}

/// <summary>
/// Dead-letter configuration for a queue.
/// </summary>
public sealed class DeadLetterTopology
{
    /// <summary>
    /// Gets or sets the dead-letter exchange name.
    /// If not specified, defaults to "{parentExchange}.dlx".
    /// </summary>
    public string? Exchange { get; set; }

    /// <summary>
    /// Gets or sets the dead-letter routing key.
    /// If not specified, defaults to "{queueName}.dlq".
    /// </summary>
    public string? RoutingKey { get; set; }

    /// <summary>
    /// Gets or sets the dead-letter queue name.
    /// If not specified, defaults to "{queueName}.dlq".
    /// </summary>
    public string? Queue { get; set; }

    /// <summary>
    /// Gets or sets the dead-letter queue message TTL (for retry delay scenarios).
    /// </summary>
    public int? MessageTtl { get; set; }
}

/// <summary>
/// Standalone queue configuration (uses default exchange).
/// </summary>
public sealed class QueueTopology
{
    /// <summary>
    /// Gets or sets the queue name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the queue is durable. Default is true.
    /// </summary>
    public bool Durable { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the queue is exclusive. Default is false.
    /// </summary>
    public bool Exclusive { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the queue is auto-deleted. Default is false.
    /// </summary>
    public bool AutoDelete { get; set; } = false;

    /// <summary>
    /// Gets or sets the message TTL in milliseconds.
    /// </summary>
    public int? MessageTtl { get; set; }

    /// <summary>
    /// Gets or sets the maximum queue length.
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets the dead-letter configuration.
    /// </summary>
    public DeadLetterTopology? DeadLetter { get; set; }

    /// <summary>
    /// Builds the queue arguments dictionary.
    /// </summary>
    internal Dictionary<string, object?> BuildArguments()
    {
        var args = new Dictionary<string, object?>();

        if (DeadLetter is not null)
        {
            args["x-dead-letter-exchange"] = DeadLetter.Exchange ?? "";
            if (!string.IsNullOrEmpty(DeadLetter.RoutingKey))
                args["x-dead-letter-routing-key"] = DeadLetter.RoutingKey;
        }

        if (MessageTtl.HasValue)
            args["x-message-ttl"] = MessageTtl.Value;

        if (MaxLength.HasValue)
            args["x-max-length"] = MaxLength.Value;

        return args;
    }
}
