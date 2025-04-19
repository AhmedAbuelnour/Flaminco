using Amqp;
using Flaminco.RabbitMQ.AMQP.Models;
using System.Text.Json;

namespace Flaminco.RabbitMQ.AMQP.Abstractions
{
    /// <summary>
    /// Represents an abstract base class for consuming messages from a message queue.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message to be consumed.</typeparam>
    public abstract class MessageConsumer<TMessage>
    {
        /// <summary>
        /// Consumes the message of type <typeparamref name="TMessage"/>.
        /// </summary>
        /// <param name="message">The received message.</param>
        /// <param name="properties">The message properties.</param>
        /// <param name="cancellationToken">A token to cancel the consumption operation.</param>
        /// <returns>A task that represents the asynchronous consume operation.</returns>
        public abstract Task Consume(TMessage message, MessageProperties properties, CancellationToken cancellationToken = default);

        /// <summary>
        /// Consumes an error message related to <typeparamref name="TMessage"/>.
        /// </summary>
        /// <param name="errorMessage">Information about the error.</param>
        /// <param name="properties">The message properties.</param>
        /// <param name="cancellationToken">A token to cancel the consumption operation.</param>
        /// <returns>A completed task by default.</returns>
        public virtual Task Consume(Exception errorMessage, MessageProperties properties, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// Processes a received AMQP message.
        /// </summary>
        /// <param name="receivedMessage">The received AMQP message.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        internal async Task ProcessMessageAsync(Message receivedMessage, CancellationToken cancellationToken = default)
        {
            if (receivedMessage == null)
                return;

            // Create message properties
            MessageProperties properties = new()
            {
                MessageId = receivedMessage.Properties?.MessageId?.ToString(),
                CorrelationId = receivedMessage.Properties?.CorrelationId?.ToString(),
                ContentType = receivedMessage.Properties?.ContentType,
            };

            // Extract application properties safely
            if (receivedMessage.ApplicationProperties?.Map != null)
            {
                foreach (var pair in receivedMessage.ApplicationProperties.Map)
                {
                    string key = pair.Key?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(key))
                    {
                        properties.ApplicationProperties[key] = pair.Value?.ToString() ?? "";
                    }
                }
            }

            try
            {
                if (receivedMessage.Body is byte[] binaryData)
                {
                    TMessage amqpMessage = JsonSerializer.Deserialize<TMessage>(binaryData, JsonSerializerOptions.Web) ?? throw new InvalidOperationException("Failed to deserialize message");

                    await Consume(amqpMessage, properties, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                await Consume(ex, properties, cancellationToken);
            }
        }
    }
}
