namespace Flaminco.RabbitMQ.AMQP.Attributes
{
    /// <summary>
    /// Specifies that a class is a consumer of messages from a specific queue.
    /// This attribute can only be applied to classes and is used to associate the class with a queue name.
    /// </summary>
    /// <param name="queue">The name of the queue that the class will consume messages from.</param>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class QueueConsumerAttribute(string queue) : Attribute
    {
        /// <summary>
        /// Gets the name of the queue that the class will consume messages from.
        /// </summary>
        public string Queue { get; } = queue;
    }
}
