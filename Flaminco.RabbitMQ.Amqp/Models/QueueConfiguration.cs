namespace Flaminco.RabbitMQ.AMQP.Models
{
    /// <summary>
    /// Represents the configuration for a message queue, including the queue name and the consumer type.
    /// </summary>
    internal class QueueConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the queue to be used.
        /// </summary>
        internal required string QueueName { get; set; }
        /// <summary>
        /// Gets or sets the type of the consumer that consumes messages from the queue.
        /// </summary>
        internal required Type ConsumerType { get; set; }
    }
}
