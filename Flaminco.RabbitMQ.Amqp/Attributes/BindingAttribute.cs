namespace Flaminco.RabbitMQ.AMQP.Attributes;

/// <summary>
/// Declares a binding between a queue and an exchange.
/// Apply to consumer classes to automatically bind the queue to an exchange.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class BindingAttribute : Attribute
{
    /// <summary>
    /// Gets the exchange name to bind to.
    /// </summary>
    public string Exchange { get; }

    /// <summary>
    /// Gets or sets the routing key for the binding. Default is empty string.
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="BindingAttribute"/> class.
    /// </summary>
    /// <param name="exchange">The exchange name to bind to.</param>
    public BindingAttribute(string exchange)
    {
        Exchange = exchange;
    }
}
