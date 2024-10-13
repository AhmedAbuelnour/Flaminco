namespace Flaminco.RabbitMQ.AMQP.Models
{
    /// <summary>
    /// Represents the configuration settings for the AMQP connection, including the host, username, and password.
    /// </summary>
    public class AMQPClientSettings
    {
        /// <summary>
        /// Gets or sets the connection string used to connect to the AMQP message broker.
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// Gets or sets the username used for authentication with the AMQP message broker.
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Gets or sets the password used for authentication with the AMQP message broker.
        /// </summary>
        public string Password { get; set; }
    }
}
