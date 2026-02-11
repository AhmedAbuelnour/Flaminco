namespace Flaminco.RabbitMQ.AMQP.HostedServices;

/// <summary>
/// Stores consumer types discovered during DI registration to ensure startup uses the same set.
/// </summary>
internal sealed class ConsumerTypeRegistry
{
    public ConsumerTypeRegistry(IReadOnlyList<Type> consumerTypes)
    {
        ConsumerTypes = consumerTypes;
    }

    public IReadOnlyList<Type> ConsumerTypes { get; }
}
