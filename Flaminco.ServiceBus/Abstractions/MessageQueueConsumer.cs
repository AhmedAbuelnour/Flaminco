

namespace Flaminco.ServiceBus.Abstractions
{
    using Azure.Messaging.ServiceBus;
    using Flaminco.ServiceBus.Options;
    using Microsoft.Extensions.Options;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    public abstract class MessageQueueConsumer
    {
        private readonly ServiceBusClient _busClient;
        private readonly ServiceBusSettings _serviceBusSettings;
        public MessageQueueConsumer(IOptions<ServiceBusSettings> options)
        {
            if (options.Value is null)
                throw new ArgumentNullException(nameof(options));

            _serviceBusSettings = options.Value;

            _busClient = new ServiceBusClient(_serviceBusSettings.ConnectionString);
        }

        public abstract string QueueName { get; set; }

        public Task Consume(Func<ProcessMessageEventArgs, Task> callback, Func<ProcessErrorEventArgs, Task> fallback, CancellationToken cancellationToken = default)
        {
            ServiceBusProcessor processor = _busClient.CreateProcessor(QueueName);

            // occurs when a message is received
            processor.ProcessMessageAsync += callback;

            // occurs when an error happen
            processor.ProcessErrorAsync += fallback;

            // Singal the processor to start listening
            return processor.StartProcessingAsync(cancellationToken);
        }
        public ValueTask Dispose()
        {
            return _busClient.DisposeAsync();
        }
    }
}