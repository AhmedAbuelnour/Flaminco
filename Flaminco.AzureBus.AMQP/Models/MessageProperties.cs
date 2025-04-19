namespace Flaminco.AzureBus.AMQP.Models
{
    /// <summary>
    /// Represents the properties of a message received from or sent to an AMQP queue.
    /// </summary>
    public sealed class MessageProperties
    {
        /// <summary>
        /// Gets or sets the unique identifier for the message.
        /// </summary>
        public string? MessageId { get; set; }

        /// <summary>
        /// Gets or sets the correlation identifier, which is used to correlate messages that are part of the same transaction or context.
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the content type of the message, which indicates the format of the message body.
        /// </summary>
        public string? ContentType { get; set; }

        /// <summary>
        /// Gets or sets the custom application properties associated with the message.
        /// </summary>
        public Dictionary<string, string> ApplicationProperties { get; set; } = [];
    }
}