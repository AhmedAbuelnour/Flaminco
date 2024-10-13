namespace Flaminco.AzureBus.AMQP.Abstractions
{
    using Azure.Messaging.ServiceBus;
    using Flaminco.AzureBus.AMQP.Options;
    using Microsoft.Extensions.Options;
    using System.Text.Json;

    /// <summary>
    /// Represents an abstract base class for publishing messages to a message queue.
    /// </summary>
    /// <remarks>
    /// Represents an abstract base class for publishing messages to a message queue.
    /// </remarks>
    /// <param name="_addressSettings">The address settings used to configure the connection to the message broker.</param>
    public abstract class MessagePublisher(IOptions<AMQPClientSettings> _addressSettings) : IAsyncDisposable
    {
        private readonly ServiceBusClient _busClient = new(_addressSettings.Value.ConnectionString);

        /// <summary>
        /// Provides default serialization options for JSON, configured with web defaults.
        /// </summary>
        private readonly JsonSerializerOptions DefaultSerializeOptions = new(JsonSerializerDefaults.Web);

        /// <summary>
        /// Gets the unique name of the message publisher, used to identify the publisher when sending messages to the queue.
        /// </summary>
        protected abstract string Name { get; }

        /// <summary>
        /// Gets the queue name where the message will be published.
        /// </summary>
        protected abstract string Queue { get; }


        /// <summary>
        /// Gets the subscription name where the message will be published.
        /// </summary>
        protected abstract string? Subscription { get; }

        /// <summary>
        /// Publishes a message asynchronously to the configured queue.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message to publish.</typeparam>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous publish operation.</returns>
        public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        {
            ServiceBusMessage serviceBusMessage = new()
            {
                Body = BinaryData.FromObjectAsJson(message, DefaultSerializeOptions),
                ContentType = "application/json",

            };

            foreach (string queue in Queues)
            {
                await _busClient.CreateSender(queue, new ServiceBusSenderOptions
                {
                    Identifier = Name,
                }).SendMessageAsync(serviceBusMessage, cancellationToken);
            }
        }

        /// <summary>
        /// Performs the task needed to clean up resources used by the Azure.Messaging.ServiceBus.ServiceBusClient, including ensuring that the client itself has been closed
        /// </summary>
        /// <returns>A task to be resolved on when the operation has completed.</returns>
        public ValueTask DisposeAsync() => _busClient.DisposeAsync();
    }
}
