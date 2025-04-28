using Azure.Messaging.ServiceBus;
using Flaminco.AzureBus.AMQP.Services;

namespace Flaminco.AzureBus.AMQP.Abstractions
{
    /// <summary>
    /// Provides an abstract base class for publishing messages to Azure Service Bus queues or topics.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This abstract base class implements the core functionality for publishing messages to Azure Service Bus.
    /// It abstracts away the details of creating and managing the underlying Service Bus sender client, allowing
    /// derived classes to focus on the specific publishing logic.
    /// </para>
    /// <para>
    /// To implement a concrete message publisher:
    /// <list type="number">
    ///   <item><description>Derive from this class</description></item>
    ///   <item><description>Implement the <see cref="EntityPath"/> property to return the queue or topic name</description></item>
    ///   <item><description>Override <see cref="IsTopic"/> to return true if publishing to a topic (default is false for queue)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The class handles the details of ensuring the queue or topic exists before attempting to publish.
    /// </para>
    /// </remarks>
    /// <param name="_connectionProvider">The Service Bus connection provider that creates and manages the underlying clients.</param>
    public abstract class MessagePublisher(AmqpConnectionProvider _connectionProvider)
    {
        /// <summary>
        /// Gets the name of the entity (queue or topic) where the message will be published.
        /// </summary>
        /// <remarks>
        /// Derived classes must implement this property to specify the name of the Azure Service Bus 
        /// queue or topic to which messages will be published.
        /// </remarks>
        /// <value>The name of the queue or topic as a string.</value>
        protected abstract string EntityPath { get; }

        /// <summary>
        /// Gets a value indicating whether the entity is a topic (true) or a queue (false).
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property determines whether messages will be published to a topic or queue.
        /// </para>
        /// <para>
        /// By default, this property returns false (queue). Override this property in derived classes 
        /// to return true when publishing to a topic.
        /// </para>
        /// </remarks>
        /// <value>True if publishing to a topic; false if publishing to a queue.</value>
        protected virtual bool IsTopic => false;

        /// <summary>
        /// Publishes a message to the Azure Service Bus queue or topic specified by <see cref="EntityPath"/>.
        /// </summary>
        /// <param name="serviceBusMessage">The <see cref="ServiceBusMessage"/> to publish, containing the message body and properties.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous publish operation.</returns>
        /// <remarks>
        /// <para>
        /// This method handles the details of:
        /// <list type="bullet">
        ///   <item><description>Creating the appropriate sender for the queue or topic</description></item>
        ///   <item><description>Ensuring the queue or topic exists before publishing</description></item>
        ///   <item><description>Publishing the message to Azure Service Bus</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// The connection provider used by this publisher will automatically create the queue or topic
        /// if it doesn't exist before attempting to publish the message.
        /// </para>
        /// </remarks>
        public async Task PublishAsync(ServiceBusMessage serviceBusMessage, CancellationToken cancellationToken = default)
        {
            // CreateSenderAsync will automatically ensure the queue/topic exists
            ServiceBusSender sender = await _connectionProvider.CreateSenderAsync(EntityPath, IsTopic, cancellationToken);

            await sender.SendMessageAsync(serviceBusMessage, cancellationToken);
        }
    }
}
