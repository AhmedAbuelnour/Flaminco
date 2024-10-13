using Azure.Messaging.ServiceBus;
using Flaminco.AzureBus.AMQP.Events;
using Flaminco.AzureBus.AMQP.Options;
using MediatR;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Flaminco.AzureBus.AMQP.Abstractions
{
    /// <summary>
    /// Represents an abstract base class for consuming messages from a message queue.
    /// </summary>
    /// <remarks>
    /// Represents an abstract base class for consuming messages from a message queue.
    /// </remarks>
    /// <param name="_addressSettings">The address settings used to configure the connection to the message broker.</param>
    /// <param name="_publisher">The event publisher for notifying when a message is received or when a fault occurs.</param>
    public abstract class MessageConsumer(IOptions<AMQPClientSettings> _addressSettings, IPublisher _publisher) : IAsyncDisposable
    {
        private readonly ServiceBusClient _busClient = new(_addressSettings.Value.ConnectionString);

        /// <summary>
        /// Provides default serialization options for JSON, configured with web defaults.
        /// </summary>
        private readonly JsonSerializerOptions DefaultSerializeOptions = new(JsonSerializerDefaults.Web);

        /// <summary>
        /// Gets the unique name of the message consumer, used to identify the consumer when consuming messages from the queue.
        /// </summary>
        protected abstract string Name { get; }


        /// <summary>
        /// Gets the name of the queue from which messages are consumed.
        /// </summary>
        protected abstract string Queue { get; }


        /// <summary>
        /// Receives a message asynchronously.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message to consume.</typeparam>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation that returns the consumed message, or <c>null</c> if the message could not be consumed.</returns>
        public Task ConsumeAsync<TMessage>(CancellationToken cancellationToken = default)
        {
            ServiceBusProcessor processor = _busClient.CreateProcessor(Queue, Name, new ServiceBusProcessorOptions
            {
                Identifier = Name,
                AutoCompleteMessages = false,
            });

            // occurs when a message is received
            processor.ProcessMessageAsync += (async (arg) =>
            {
                await _publisher.Publish(new MessageReceivedEvent<TMessage>
                {
                    Message = arg.Message.Body.ToObjectFromJson<TMessage>(DefaultSerializeOptions)
                }, cancellationToken);

                await arg.CompleteMessageAsync(arg.Message, cancellationToken);
            });

            // occurs when an error happen
            processor.ProcessErrorAsync += (async (arg) =>
            {
                await _publisher.Publish(new MessageFaultEvent<TMessage>
                {
                    Name = Name,
                    Queue = Queue,
                    Exception = arg.Exception
                }, cancellationToken);
            });

            // Signal the processor to start listening
            processor.StartProcessingAsync(cancellationToken);

            return Task.Delay(Timeout.Infinite, cancellationToken);
        }


        /// <summary>
        /// Performs the task needed to clean up resources used by the Azure.Messaging.ServiceBus.ServiceBusClient, including ensuring that the client itself has been closed
        /// </summary>
        /// <returns>A task to be resolved on when the operation has completed.</returns>
        public ValueTask DisposeAsync() => _busClient.DisposeAsync();

    }

}
