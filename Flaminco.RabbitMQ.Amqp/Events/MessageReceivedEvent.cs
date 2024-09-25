namespace Flaminco.RabbitMQ.AMQP.Events
{
    using MediatR;

    /// <summary>
    /// Represents an event triggered when a message is successfully received from the queue.
    /// </summary>
    /// <typeparam name="TMessage">The type of the received message.</typeparam>
    public sealed class MessageReceivedEvent<TMessage> : INotification
    {
        /// <summary>
        /// Gets or sets the received message from the queue.
        /// </summary>
        public required TMessage Message { get; set; }
    }

    /// <summary>
    /// Defines a handler for processing received messages.
    /// </summary>
    /// <typeparam name="TMessage">The type of the received message.</typeparam>
    public interface IMessageReceivedHandler<TMessage> : INotificationHandler<MessageReceivedEvent<TMessage>>;
}
