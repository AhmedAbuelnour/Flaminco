namespace Flaminco.AzureBus.AMQP.Attributes
{
    /// <summary>
    /// Specifies that a class is a consumer of messages from a specific Azure Service Bus topic subscription.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This attribute is used by the Azure Service Bus AMQP infrastructure to automatically discover and register
    /// message consumers during application startup. It identifies classes that should process messages from
    /// a specific topic subscription.
    /// </para>
    /// <para>
    /// When applied to a class that inherits from <see cref="Abstractions.MessageConsumer{TMessage}"/>, this attribute
    /// enables the automatic registration of the class as a consumer for the specified topic subscription.
    /// </para>
    /// <para>
    /// Example usage:
    /// <code>
    /// [TopicConsumer("notifications-topic", "email-notifications")]
    /// public class EmailNotificationProcessor : MessageConsumer&lt;NotificationMessage&gt;
    /// {
    ///     public override Task ConsumeAsync(NotificationMessage message, ServiceBusReceivedMessage serviceBusMessage, CancellationToken cancellationToken = default)
    ///     {
    ///         // Process the notification message for email delivery
    ///         return Task.CompletedTask;
    ///     }
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    /// <seealso cref="QueueConsumerAttribute"/>
    /// <seealso cref="Abstractions.MessageConsumer{TMessage}"/>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class TopicConsumerAttribute(string topic, string subscription) : Attribute
    {
        /// <summary>
        /// Gets the name of the Azure Service Bus topic that the decorated class will consume messages from.
        /// </summary>
        /// <value>
        /// The topic name as a string. Cannot be null or empty.
        /// </value>
        /// <exception cref="ArgumentNullException">Thrown when the topic name is null.</exception>
        public string Topic { get; } = topic ?? throw new ArgumentNullException(nameof(topic));

        /// <summary>
        /// Gets the name of the subscription within the topic that the decorated class will consume messages from.
        /// </summary>
        /// <value>
        /// The subscription name as a string. Cannot be null or empty.
        /// </value>
        /// <exception cref="ArgumentNullException">Thrown when the subscription name is null.</exception>
        /// <remarks>
        /// A topic subscription acts as a virtual queue within a topic, allowing multiple consumers
        /// to receive different messages from the same topic based on filter rules.
        /// </remarks>
        public string Subscription { get; } = subscription ?? throw new ArgumentNullException(nameof(subscription));
    }
}