namespace Flaminco.RabbitMQ.AMQP.Configuration;

/// <summary>
/// Represents an exchange declaration configuration.
/// </summary>
public sealed class ExchangeDeclaration
{
    /// <summary>
    /// Gets or sets the exchange name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the exchange type (direct, fanout, topic, headers).
    /// </summary>
    public string Type { get; set; } = ExchangeTypes.Direct;

    /// <summary>
    /// Gets or sets whether the exchange survives broker restarts.
    /// </summary>
    public bool Durable { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the exchange is deleted when the last queue is unbound.
    /// </summary>
    public bool AutoDelete { get; set; } = false;

    /// <summary>
    /// Gets or sets additional exchange arguments.
    /// </summary>
    public Dictionary<string, object?> Arguments { get; set; } = [];
}

/// <summary>
/// Standard exchange types.
/// </summary>
public static class ExchangeTypes
{
    /// <summary>
    /// Direct exchange - routes messages to queues based on exact routing key match.
    /// </summary>
    public const string Direct = "direct";

    /// <summary>
    /// Fanout exchange - routes messages to all bound queues (ignores routing key).
    /// </summary>
    public const string Fanout = "fanout";

    /// <summary>
    /// Topic exchange - routes messages based on routing key patterns.
    /// </summary>
    public const string Topic = "topic";

    /// <summary>
    /// Headers exchange - routes messages based on header attributes.
    /// </summary>
    public const string Headers = "headers";
}
