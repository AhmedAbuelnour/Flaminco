namespace Flaminco.RabbitMQ.AMQP.Abstractions
{
    using MassTransit;

    /// <summary>
    /// Represents an abstract base class for publishing messages to a message queue.
    /// </summary>
    /// <param name="sendEndpointProvider">The endpoint provider used to send messages to a specific queue.</param>
    public abstract class MessagePublisher(ISendEndpointProvider sendEndpointProvider)
    {
        /// <summary>
        /// Gets the name of the queue where the message will be published.
        /// </summary>
        protected abstract string Queue { get; }

        /// <summary>
        /// Publishes a message to the specified queue using MassTransit.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message being published. Must implement <see cref="IMessage"/>.</typeparam>
        /// <param name="message">The message to be published.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the publish operation.</param>
        /// <returns>A task that represents the asynchronous publish operation.</returns>
        public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : class, IMessage
        {
            ISendEndpoint endpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{Queue}"));

            await endpoint.Send(message, cancellationToken);
        }
    }
}
