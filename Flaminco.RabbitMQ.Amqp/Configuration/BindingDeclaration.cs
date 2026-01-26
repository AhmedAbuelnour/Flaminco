namespace Flaminco.RabbitMQ.AMQP.Configuration;

/// <summary>
/// Represents a binding between an exchange and a queue.
/// </summary>
public sealed class BindingDeclaration
{
    /// <summary>
    /// Gets or sets the source exchange name.
    /// </summary>
    public required string Exchange { get; set; }

    /// <summary>
    /// Gets or sets the destination queue name.
    /// </summary>
    public required string Queue { get; set; }

    /// <summary>
    /// Gets or sets the routing key for the binding.
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional binding arguments.
    /// </summary>
    public Dictionary<string, object?> Arguments { get; set; } = [];
}
