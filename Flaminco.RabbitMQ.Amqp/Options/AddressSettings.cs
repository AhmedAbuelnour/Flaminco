namespace Flaminco.RabbitMQ.AMQP.Options
{
    /// <summary>
    /// Represents the configuration settings for the AMQP connection, including the connection string.
    /// </summary>
    public class AddressSettings
    {
        /// <summary>
        /// Gets or sets the connection string used to connect to the AMQP message broker.
        /// </summary>
        public required string ConnectionString { get; set; }
    }
}
