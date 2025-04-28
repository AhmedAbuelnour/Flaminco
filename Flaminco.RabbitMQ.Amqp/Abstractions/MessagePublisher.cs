namespace Flaminco.RabbitMQ.AMQP.Abstractions
{
    using Flaminco.RabbitMQ.AMQP.Services;
    using global::RabbitMQ.Client;
    using System.Text.Json;

    /// <summary>
    /// Represents an abstract base class for publishing messages to a message queue.
    /// </summary>
    /// <param name="connectionProvider">The AMQP connection provider.</param>
    public abstract class MessagePublisher(AmqpConnectionProvider connectionProvider)
    {
        /// <summary>
        /// Gets the name of the queue where the message will be published.
        /// </summary>
        protected abstract string Queue { get; }

        /// <summary>
        /// Publishes a message to the specified queue using RabbitMQ.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message being published.</typeparam>
        /// <param name="message">The message to be published.</param>
        /// <param name="properties">Optional parameters to customize the message sending process.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the publish operation.</param>
        /// <returns>A task that represents the asynchronous publish operation.</returns>
        public async Task PublishAsync<TMessage>(TMessage message, BasicProperties properties, CancellationToken cancellationToken = default)
        {
            // Get channel for publishing
            using IChannel channel = await connectionProvider.CreateChannelAsync(cancellationToken);

            // Ensure the queue exists by declaring it (this will create it if it doesn't exist)
            await channel.QueueDeclareAsync(
                queue: Queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            // Publish the message
            await channel.BasicPublishAsync(exchange: string.Empty,
                                            routingKey: Queue,
                                            mandatory: false,
                                            basicProperties: properties,
                                            body: JsonSerializer.SerializeToUtf8Bytes(message, JsonSerializerOptions.Web),
                                            cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Publishes a message to the specified queue using RabbitMQ.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message being published.</typeparam>
        /// <param name="message">The message to be published.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the publish operation.</param>
        /// <returns>A task that represents the asynchronous publish operation.</returns>
        public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        {
            return PublishAsync(message, new()
            {
                ContentType = "application/json",
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                MessageId = Guid.NewGuid().ToString(),
            }, cancellationToken);
        }
    }
}
