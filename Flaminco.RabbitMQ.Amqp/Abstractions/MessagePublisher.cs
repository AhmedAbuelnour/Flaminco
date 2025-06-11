namespace Flaminco.RabbitMQ.AMQP.Abstractions
{
    using MassTransit;

    /// <summary>
    /// Represents an abstract base class for publishing messages to a queue using MassTransit.
    /// </summary>
    /// <param name="_sendEndpointProvider">The send endpoint provider.</param>
    /// <param name="_publishEndpoint">The publish endpoint.</param>
    public abstract class MessagePublisher(ISendEndpointProvider _sendEndpointProvider,
                                           IPublishEndpoint _publishEndpoint)
    {
        /// <summary>
        /// Gets the name of the queue where the message will be published.
        /// </summary>
        protected abstract string Queue { get; }

        /// <summary>
        /// Publishes a message to the specified queue using MassTransit.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message being published.</typeparam>
        /// <param name="message">The message to be published.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the publish operation.</param>
        /// <returns>A task that represents the asynchronous publish operation.</returns>
        public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
            where TMessage : class
        {
            ISendEndpoint endpoint = await _sendEndpointProvider.GetSendEndpoint(new($"queue:{Queue}"));
            await endpoint.Send(message, cancellationToken);
        }
    }
}
