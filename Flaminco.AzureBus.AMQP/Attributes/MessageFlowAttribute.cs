namespace Flaminco.AzureBus.AMQP.Attributes
{
    /// <summary>
    ///     Specifies the queue and message type that a class will consume messages from in a message flow.
    ///     This attribute is intended for use with classes that represent message flows in a message queue system.
    /// </summary>
    /// <remarks>
    ///     Initializes a new instance of the <see cref="MessageFlowAttribute"/> class with the specified queue name and message type.
    /// </remarks>
    /// <param name="queue">The name of the queue that the class will consume messages from.</param>
    /// <param name="messageType">The type of the message that the class will consume from the queue.</param>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class MessageFlowAttribute(string queue, Type messageType) : Attribute
    {

        /// <summary>
        ///     Gets the name of the queue that the class will consume messages from.
        /// </summary>
        public string Queue { get; } = queue;

        /// <summary>
        ///     Gets the type of the message that the class will consume from the queue.
        /// </summary>
        public Type MessageType { get; } = messageType;
    }
}
