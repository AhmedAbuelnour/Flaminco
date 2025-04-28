using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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
        public abstract Task ConsumeAsync(TMessage message, IReadOnlyBasicProperties properties, CancellationToken cancellationToken = default);

        /// <summary>
        /// Handles errors that occur during message consumption.
        /// </summary>
        /// <param name="error">The exception that occurred.</param>
        /// <param name="properties">The message properties.</param>
        /// <param name="cancellationToken">A token to cancel the consumption operation.</param>
        /// <returns>A completed task by default.</returns>
        public virtual Task ConsumeAsync(Exception error, IReadOnlyBasicProperties properties, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// Processes a received RabbitMQ message.
        /// </summary>
        /// <param name="args">The received RabbitMQ message event args.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        internal async Task ProcessMessageAsync(BasicDeliverEventArgs args, CancellationToken cancellationToken = default)
        {
            if (args == null)
                return;

            try
            {
                if (args.Body.Length > 0)
                {
                    TMessage rabbitMessage = JsonSerializer.Deserialize<TMessage>(args.Body.Span, JsonSerializerOptions.Web) ?? throw new InvalidOperationException("Failed to deserialize message");

                    await ConsumeAsync(rabbitMessage, args.BasicProperties, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                await ConsumeAsync(ex, args.BasicProperties, cancellationToken);
            }
        }
    }
}
