namespace Flaminco.RabbitMQ.AMQP.Models
{
    /// <summary>
    /// Provides options for publishing a message, allowing customization of various properties.
    /// </summary>
    public class MessagePublishOptions
    {
        /// <summary>
        /// Gets or sets the unique identifier for the message.
        /// </summary>
        public Guid? MessageId { get; set; }

        /// <summary>
        /// Gets or sets the correlation identifier, which is used to correlate messages that are part of the same transaction or context.
        /// </summary>
        public Guid? CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the content type of the message, which indicates the format of the message body.
        /// </summary>
        public string? ContentType { get; set; }

        /// <summary>
        /// Gets or sets the time span after which the message expires.
        /// </summary>
        public TimeSpan? TimeToLive { get; set; }

        /// <summary>
        /// Gets or sets the partition key used to route the message to a specific partition.
        /// </summary>
        public string? PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the custom application properties associated with the message.
        /// </summary>
        public Dictionary<string, string>? ApplicationProperties { get; set; } = [];
    }
}
