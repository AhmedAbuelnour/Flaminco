namespace Flaminco.RabbitMQ.AMQP.Models
{
    /// <summary>
    /// Represents the configuration settings for the AMQP connection, including the host, username, password, and optional retry configurations.
    /// </summary>
    public sealed class AmqpClientSettings
    {
        /// <summary>
        /// Gets or sets the AMQP connection host URL.
        /// </summary>
        /// <remarks>
        /// The format should be: "amqp://hostname:5672" or "amqp://hostname:port"
        /// </remarks>
        public string Host { get; set; } = "amqp://localhost:5672";

        /// <summary>
        /// Gets or sets the username for the AMQP connection.
        /// </summary>
        public string Username { get; set; } = "guest";

        /// <summary>
        /// Gets or sets the password for the AMQP connection.
        /// </summary>
        public string Password { get; set; } = "guest";

        /// <summary>
        /// Gets or sets the retry count for AMQP operations.
        /// </summary>
        public int? RetryCount { get; set; }

        /// <summary>
        /// Gets or sets the retry interval in milliseconds.
        /// </summary>
        public TimeSpan? RetryInterval { get; set; }

        /// <summary>
        /// Gets or sets the connection idle timeout.
        /// </summary>
        public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromSeconds(60);
    }
}
