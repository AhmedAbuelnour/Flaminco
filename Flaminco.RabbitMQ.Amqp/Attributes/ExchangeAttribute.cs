using Flaminco.RabbitMQ.AMQP.Configuration;

namespace Flaminco.RabbitMQ.AMQP.Attributes;

/// <summary>
/// Declares an exchange that should be created at startup.
/// Apply to consumer or publisher classes to automatically declare exchanges.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class ExchangeAttribute : Attribute
{
    /// <summary>
    /// Gets the exchange name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the exchange type. Default is "direct".
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
    /// Initializes a new instance of the <see cref="ExchangeAttribute"/> class.
    /// </summary>
    /// <param name="name">The exchange name.</param>
    public ExchangeAttribute(string name)
    {
        Name = name;
    }
}
