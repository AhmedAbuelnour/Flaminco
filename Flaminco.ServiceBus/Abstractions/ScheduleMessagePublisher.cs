namespace Flaminco.ServiceBus.Abstractions
{
    using Azure.Messaging.ServiceBus;
    using Flaminco.ServiceBus.Options;
    using Microsoft.Extensions.Options;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    public abstract class ScheduleMessagePublisher : MessagePublisher
    {
        public ScheduleMessagePublisher(IOptions<ServiceBusSettings> options) : base(options)
        {
        }
        public required DateTimeOffset Offset { get; init; }
        public new Task Publish<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        {
            ServiceBusMessage serviceBusMessage = BusMessageBuilder(message);

            return _busClient.CreateSender(QueueOrTopicName).ScheduleMessageAsync(serviceBusMessage, Offset, cancellationToken);
        }
    }
}