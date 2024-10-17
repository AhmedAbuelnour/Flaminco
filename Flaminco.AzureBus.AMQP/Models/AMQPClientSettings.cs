namespace Flaminco.AzureBus.AMQP.Models
{
    /// <summary>
    /// Represents the configuration settings for the AMQP connection, including the host, retry options, and optional retry configurations.
    /// </summary>
    public class AMQPClientSettings
    {
        /// <summary>
        /// Gets or sets the host address used to connect to the AMQP message broker.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the number of retry attempts to be made in case of connection failure.
        /// If null, no retries will be attempted.
        /// </summary>
        public int? RetryCount { get; set; }

        /// <summary>
        /// Gets or sets the time interval between retry attempts in case of connection failure.
        /// If null, no retry interval will be applied.
        /// </summary>
        public TimeSpan? RetryInterval { get; set; }
    }

}
