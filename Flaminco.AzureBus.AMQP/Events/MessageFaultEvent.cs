using MediatR;

namespace Flaminco.AzureBus.AMQP.Events
{
    /// <summary>
    /// Represents an event triggered when a fault or error occurs while processing a message from the queue.
    /// This event provides information about the consumer, queue, and the message that caused the fault.
    /// </summary>
    public sealed class MessageFaultEvent<TMessage> : INotification
    {
        /// <summary>
        /// Gets or sets the name of the message consumer where the fault occurred.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the queue where the fault occurred.
        /// </summary>
        public required string Queue { get; set; }

        /// <summary>
        /// Gets or sets the exception that caused the fault, if available.
        /// </summary>
        public required Exception? Exception { get; set; }
    }

    /// <summary>
    /// Defines a handler for processing fault events that occur during message consumption.
    /// </summary>
    public interface IMessageFaultHandler<TMessage> : INotificationHandler<MessageFaultEvent<TMessage>>;

}
