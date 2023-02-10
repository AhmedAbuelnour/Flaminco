namespace Flaminco.ServiceBus.Abstractions
{
    public interface IServiceBusLocator
    {
        MessagePublisher? GetPublisher<TPublisher>() where TPublisher : MessagePublisher;
        MessageQueueConsumer? GetQueueConsumer<TConsumer>() where TConsumer : MessageQueueConsumer;
        MessageTopicConsumer? GetTopicConsumer<TConsumer>() where TConsumer : MessageTopicConsumer;
    }
}
