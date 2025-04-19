namespace Flaminco.AzureBus.AMQP.Abstractions
{
    using Amqp;
    using Amqp.Framing;
    using Flaminco.AzureBus.AMQP.Models;
    using Flaminco.AzureBus.AMQP.Services;
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
        /// Identify if the message should be sent to a topic, default is false.
        /// </summary>
        protected abstract bool IsTopic { get; }

        /// <summary>
        /// Publishes a message to the specified queue using AMQP 1.0.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message being published.</typeparam>
        /// <param name="message">The message to be published.</param>
        /// <param name="options">Optional parameters to customize the message sending process.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the publish operation.</param>
        /// <returns>A task that represents the asynchronous publish operation.</returns>
        public async Task PublishAsync<TMessage>(TMessage message, MessagePublishOptions? options = default, CancellationToken cancellationToken = default)
        {
            // Get session for this queue
            Session session = await connectionProvider.GetSessionAsync(Queue);

            // Create a sender for the queue
            SenderLink sender = new(session, $"sender-{Guid.NewGuid()}", Queue);

            try
            {
                // Create an AMQP message
                Message amqpMessage = new()
                {
                    BodySection = new Data
                    {
                        Binary = JsonSerializer.SerializeToUtf8Bytes(message, JsonSerializerOptions.Web)
                    },
                    Properties = new Properties
                    {
                        ContentType = "application/json",
                        CreationTime = DateTime.UtcNow,
                    }
                };

                // Apply options if provided
                if (options != null)
                {
                    AttachProperties(amqpMessage, options);
                }

                // Send the message
                await sender.SendAsync(amqpMessage);
            }
            finally
            {
                // Close and dispose the sender
                await sender.CloseAsync();
            }
        }

        /// <summary>
        /// Publishes a message to the specified queue using AMQP 1.0.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message being published.</typeparam>
        /// <param name="message">The message to be published.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the publish operation.</param>
        /// <returns>A task that represents the asynchronous publish operation.</returns>
        public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        {
            return PublishAsync(message, null, cancellationToken);
        }

        private static void AttachProperties(Message message, MessagePublishOptions options)
        {
            if (options == null)
                return;

            // Set message properties
            message.Properties ??= new Properties();

            if (options.MessageId.HasValue)
                message.Properties.MessageId = options.MessageId.Value.ToString();

            if (options.CorrelationId.HasValue)
                message.Properties.CorrelationId = options.CorrelationId.Value.ToString();

            if (!string.IsNullOrWhiteSpace(options.ContentType))
                message.Properties.ContentType = options.ContentType;

            if (options.TimeToLive.HasValue)
                message.Properties.AbsoluteExpiryTime = DateTime.UtcNow.Add(options.TimeToLive.Value);

            // Set application properties
            if (options.ApplicationProperties != null && options.ApplicationProperties.Count > 0)
            {
                message.ApplicationProperties = new ApplicationProperties();
                foreach (var prop in options.ApplicationProperties)
                {
                    message.ApplicationProperties[prop.Key] = prop.Value;
                }
            }

            // Note: AMQP 1.0 doesn't have a direct equivalent of MassTransit's PartitionKey
            // but you could add it as an application property if needed
            if (!string.IsNullOrWhiteSpace(options.PartitionKey))
            {
                message.ApplicationProperties ??= new ApplicationProperties();

                message.ApplicationProperties["PartitionKey"] = options.PartitionKey;
            }
        }
    }
}
