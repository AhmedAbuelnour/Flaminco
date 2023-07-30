namespace Flaminco.ServiceBus.Abstractions
{
    using Azure.Messaging.ServiceBus;
    using Flaminco.ServiceBus.Options;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    public abstract class MessagePublisher
    {
        protected readonly ServiceBusClient _busClient;
        private readonly ServiceBusSettings _serviceBusSettings;
        public MessagePublisher(IOptions<ServiceBusSettings> options)
        {
            if (options.Value is null)
                throw new ArgumentNullException(nameof(options));

            _serviceBusSettings = options.Value;

            _busClient = new ServiceBusClient(_serviceBusSettings.ConnectionString);
        }
        public abstract string QueueOrTopicName { get; init; }
        public string ContentType { get; init; } = "application/json";
        public Guid? MessageId { get; init; }
        public Guid? CorrelationId { get; init; }
        public string? To { get; init; }
        public string? Subject { get; init; }
        public string? SessionId { get; init; }
        public TimeSpan? TimeToLive { get; init; }
        public Dictionary<string, object>? CustomFilters { get; set; }
        public Task Publish<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        {
            ServiceBusMessage serviceBusMessage = BusMessageBuilder(message);

            return _busClient.CreateSender(QueueOrTopicName).SendMessageAsync(serviceBusMessage, cancellationToken);
        }
        protected ServiceBusMessage BusMessageBuilder<TMessage>(TMessage message)
        {
            ServiceBusMessage serviceBusMessage = new()
            {
                Body = new BinaryData(JsonSerializer.SerializeToUtf8Bytes(message)),
                ContentType = ContentType,
            };

            // Checks for filter
            if (CustomFilters?.Any() ?? false)
                foreach (var filter in CustomFilters)
                    serviceBusMessage.ApplicationProperties[filter.Key] = filter.Value;

            if (MessageId is not null)
                serviceBusMessage.MessageId = MessageId.Value.ToString();

            if (CorrelationId is not null)
                serviceBusMessage.CorrelationId = CorrelationId.Value.ToString();

            if (SessionId is not null)
                serviceBusMessage.SessionId = SessionId!;

            if (To is not null)
                serviceBusMessage.To = To!;

            if (TimeToLive is not null)
                serviceBusMessage.TimeToLive = TimeToLive.Value;

            if (Subject is not null)
                serviceBusMessage.Subject = Subject!;

            return serviceBusMessage;
        }
        public ValueTask Dispose()
        {
            return _busClient.DisposeAsync();
        }
    }

}