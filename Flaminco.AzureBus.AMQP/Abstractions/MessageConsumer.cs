using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace Flaminco.AzureBus.AMQP.Abstractions
{
    /// <summary>
    /// Provides an abstract base class for consuming messages from Azure Service Bus queues and topics.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to be consumed.</typeparam>
    /// <remarks>
    /// <para>
    /// This abstract base class implements the core functionality for processing and deserializing messages
    /// received from Azure Service Bus. It handles the common patterns of message processing while allowing
    /// derived classes to implement the specific consumption logic.
    /// </para>
    /// <para>
    /// The class provides error handling capabilities by catching exceptions that occur during message 
    /// processing and redirecting them to a virtual error handling method that can be overridden.
    /// </para>
    /// <para>
    /// To implement a concrete message consumer:
    /// <list type="number">
    ///   <item><description>Derive from this class, specifying the message type</description></item>
    ///   <item><description>Implement the <see cref="ConsumeAsync(TMessage, ServiceBusReceivedMessage, CancellationToken)"/> method</description></item>
    ///   <item><description>Optionally override <see cref="ConsumeAsync(Exception, ServiceBusReceivedMessage, CancellationToken)"/> for custom error handling</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public abstract class MessageConsumer<TMessage>
    {
        /// <summary>
        /// Processes a message of type <typeparamref name="TMessage"/> received from Azure Service Bus.
        /// </summary>
        /// <param name="message">The deserialized message of type <typeparamref name="TMessage"/>.</param>
        /// <param name="serviceBusMessage">The original <see cref="ServiceBusReceivedMessage"/> containing metadata and properties.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous consume operation.</returns>
        /// <remarks>
        /// Implement this method to define the specific logic for processing the message in derived classes.
        /// This method will only be called if the message body can be successfully deserialized to <typeparamref name="TMessage"/>.
        /// </remarks>
        public abstract Task ConsumeAsync(TMessage message, ServiceBusReceivedMessage serviceBusMessage, CancellationToken cancellationToken = default);

        /// <summary>
        /// Processes an error that occurred while consuming a message.
        /// </summary>
        /// <param name="error">The exception that was thrown during message processing.</param>
        /// <param name="serviceBusMessage">The original <see cref="ServiceBusReceivedMessage"/> that caused the error.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous error handling operation.</returns>
        /// <remarks>
        /// <para>
        /// This method is called when an exception occurs during the processing of a message.
        /// The default implementation does nothing and returns a completed task.
        /// </para>
        /// <para>
        /// Override this method in derived classes to implement custom error handling logic, such as:
        /// <list type="bullet">
        ///   <item><description>Logging the error</description></item>
        ///   <item><description>Sending the message to a dead-letter queue</description></item>
        ///   <item><description>Notifying external systems</description></item>
        ///   <item><description>Attempting to recover or retry the operation</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// Note that after this method completes, the original exception will be re-thrown to allow
        /// the Azure Service Bus processor to apply its retry policy.
        /// </para>
        /// </remarks>
        public virtual Task ConsumeAsync(Exception error, ServiceBusReceivedMessage serviceBusMessage, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        /// <summary>
        /// Internal method that processes a received Service Bus message by deserializing it and routing to the appropriate handler.
        /// </summary>
        /// <param name="message">The received <see cref="ServiceBusReceivedMessage"/> to process.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous processing operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the message body cannot be deserialized to <typeparamref name="TMessage"/>.</exception>
        /// <remarks>
        /// This internal method is called by the Azure Service Bus processor and handles:
        /// <list type="bullet">
        ///   <item><description>Deserialization of the message body to <typeparamref name="TMessage"/></description></item>
        ///   <item><description>Calling the appropriate <see cref="ConsumeAsync(TMessage, ServiceBusReceivedMessage, CancellationToken)"/> method</description></item>
        ///   <item><description>Error handling by catching exceptions and routing them to <see cref="ConsumeAsync(Exception, ServiceBusReceivedMessage, CancellationToken)"/></description></item>
        /// </list>
        /// </remarks>
        internal async Task ProcessMessageAsync(ServiceBusReceivedMessage message, CancellationToken cancellationToken = default)
        {
            if (message == null)
                return;

            try
            {
                TMessage messageBody = message.Body.ToObjectFromJson<TMessage>(JsonSerializerOptions.Web) ?? throw new InvalidOperationException("Message body is null.");

                await ConsumeAsync(messageBody, message, cancellationToken);
            }
            catch (Exception ex)
            {
                // Handle exceptions by calling the error consumer
                await ConsumeAsync(ex, message, cancellationToken);

                // Re-throw to allow the processor to handle the error according to its retry policy
                throw;
            }
        }
    }
}
