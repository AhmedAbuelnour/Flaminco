namespace Flaminco.RabbitMQ.AMQP.Abstractions
{

    /// <summary>
    /// Provides a mechanism to locate and retrieve instances of AMQP message publishers and consumers.
    /// </summary>
    public interface IAMQPLocator
    {
        /// <summary>
        /// Retrieves an instance of a message publisher of the specified type.
        /// </summary>
        /// <typeparam name="TPublisher">The type of the message publisher to retrieve. Must inherit from <see cref="MessagePublisher"/>.</typeparam>
        /// <returns>An instance of the specified message publisher, or <c>null</c> if no publisher of the specified type is found.</returns>
        MessagePublisher GetPublisher<TPublisher>() where TPublisher : MessagePublisher;

        /// <summary>
        /// Retrieves an instance of a message consumer of the specified type.
        /// </summary>
        /// <typeparam name="TConsumer">The type of the message consumer to retrieve. Must inherit from <see cref="MessageConsumer"/>.</typeparam>
        /// <returns>An instance of the specified message consumer, or <c>null</c> if no consumer of the specified type is found.</returns>
        MessageConsumer GetConsumer<TConsumer>() where TConsumer : MessageConsumer;
    }
}
