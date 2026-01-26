namespace Flaminco.RabbitMQ.AMQP.Attributes;

/// <summary>
/// Declares a queue that should be created at startup.
/// Apply to consumer classes to automatically declare and consume from the queue.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class QueueAttribute : Attribute
{
    /// <summary>
    /// Gets the queue name.
    /// </summary>
    public string Name { get; }

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
    /// Gets or sets the prefetch count for this consumer. Default uses global setting.
    /// </summary>
    public ushort PrefetchCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets whether to automatically acknowledge messages. Default is false (manual acknowledgment).
    /// When true, messages are automatically removed from the queue upon delivery.
    /// When false, you must manually acknowledge messages using the context.
    /// </summary>
    public bool AutoAck { get; set; } = false;

    /// <summary>
    /// Gets or sets the dead-letter exchange name.
    /// </summary>
    public string? DeadLetterExchange { get; set; }

    /// <summary>
    /// Gets or sets the dead-letter routing key.
    /// </summary>
    public string? DeadLetterRoutingKey { get; set; }

    /// <summary>
    /// Gets or sets the message TTL in milliseconds.
    /// </summary>
    public int MessageTtl { get; set; } = 0;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts. Default is 3.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets whether to requeue failed messages. Default is false.
    /// </summary>
    public bool RequeueOnFailure { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueueAttribute"/> class.
    /// </summary>
    /// <param name="name">The queue name.</param>
    public QueueAttribute(string name)
    {
        Name = name;
    }
}
