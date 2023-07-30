
namespace Flaminco.ServiceBus.Abstractions
{
    using Azure.Messaging.ServiceBus;
    using Flaminco.ServiceBus.Options;
    using Microsoft.Extensions.Options;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    public abstract class MessageTopicConsumer
    {
        private readonly ServiceBusClient _busClient;
        private readonly ServiceBusSettings _serviceBusSettings;
        public MessageTopicConsumer(IOptions<ServiceBusSettings> options)
        {
            ArgumentNullException.ThrowIfNull(options.Value);

            _serviceBusSettings = options.Value;

            _busClient = new ServiceBusClient(_serviceBusSettings.ConnectionString);
        }

        public abstract string TopicName { get; init; }
        public abstract string SubscriptionName { get; init; }

        public Task Consume(Func<ProcessMessageEventArgs, Task> callback, Func<ProcessErrorEventArgs, Task> fallback, CancellationToken cancellationToken = default)
        {
            ServiceBusProcessor processor = _busClient.CreateProcessor(TopicName, SubscriptionName);

            // occurs when a message is received
            processor.ProcessMessageAsync += callback;

            // occurs when an error happen
            processor.ProcessErrorAsync += fallback;

            // Signal the processor to start listening
            return processor.StartProcessingAsync(cancellationToken);
        }
        public ValueTask Dispose()
        {
            return _busClient.DisposeAsync();
        }
    }

}