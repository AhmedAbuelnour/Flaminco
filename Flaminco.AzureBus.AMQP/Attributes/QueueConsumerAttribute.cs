namespace Flaminco.AzureBus.AMQP.Attributes
{
    /// <summary>
    /// Specifies that a class is a consumer of messages from a specific Azure Service Bus queue.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This attribute is used by the Azure Service Bus AMQP infrastructure to automatically discover and register
    /// message consumers during application startup. It identifies classes that should process messages from
    /// a specific queue.
    /// </para>
    /// <para>
    /// When applied to a class that inherits from <see cref="Abstractions.MessageConsumer{TMessage}"/>, this attribute
    /// enables the automatic registration of the class as a consumer for the specified queue.
    /// </para>
    /// <para>
    /// Example usage:
    /// <code>
    /// [QueueConsumer("orders-queue")]
    /// public class OrderProcessor : MessageConsumer&lt;OrderMessage&gt;
    /// {
    ///     public override Task ConsumeAsync(OrderMessage message, ServiceBusReceivedMessage serviceBusMessage, CancellationToken cancellationToken = default)
    ///     {
    ///         // Process the order message
    ///         return Task.CompletedTask;
    ///     }
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    /// <seealso cref="TopicConsumerAttribute"/>
    /// <seealso cref="Abstractions.MessageConsumer{TMessage}"/>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class QueueConsumerAttribute(string queue) : Attribute
    {
        /// <summary>
        /// Gets the name of the Azure Service Bus queue that the decorated class will consume messages from.
        /// </summary>
        /// <value>
        /// The queue name as a string. Cannot be null or empty.
        /// </value>
        /// <exception cref="ArgumentNullException">Thrown when the queue name is null.</exception>
        public string Queue { get; } = queue ?? throw new ArgumentNullException(nameof(queue));
    }
}
