namespace Flaminco.ServiceBus.Implementation
{
    using Flaminco.ServiceBus.Abstractions;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Linq;

    public class ServiceBusLocator(IServiceProvider _serviceProvider) : IServiceBusLocator
    {
        public MessagePublisher GetPublisher<TPublisher>() where TPublisher : MessagePublisher
        {
            return _serviceProvider.GetServices<MessagePublisher>().Single(a => a.GetType() == typeof(TPublisher));
        }

        public MessageQueueConsumer GetQueueConsumer<TConsumer>() where TConsumer : MessageQueueConsumer
        {
            return _serviceProvider.GetServices<MessageQueueConsumer>().Single(a => a.GetType() == typeof(TConsumer));
        }

        public MessageTopicConsumer GetTopicConsumer<TConsumer>() where TConsumer : MessageTopicConsumer
        {
            return _serviceProvider.GetServices<MessageTopicConsumer>().Single(a => a.GetType() == typeof(TConsumer));
        }
    }
}
