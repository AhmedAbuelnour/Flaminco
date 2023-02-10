namespace Flaminco.ServiceBus.Implementation
{
    using Flaminco.ServiceBus.Abstractions;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Linq;

    public class ServiceBusLocator : IServiceBusLocator
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceBusLocator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        public MessagePublisher? GetPublisher<TPublisher>() where TPublisher : MessagePublisher
        {
            return _serviceProvider.GetServices<MessagePublisher>()?.FirstOrDefault(a => a.GetType() == typeof(TPublisher));
        }

        public MessageQueueConsumer? GetQueueConsumer<TConsumer>() where TConsumer : MessageQueueConsumer
        {
            return _serviceProvider.GetServices<MessageQueueConsumer>()?.FirstOrDefault(a => a.GetType() == typeof(TConsumer));
        }

        public MessageTopicConsumer? GetTopicConsumer<TConsumer>() where TConsumer : MessageTopicConsumer
        {
            return _serviceProvider.GetServices<MessageTopicConsumer>()?.FirstOrDefault(a => a.GetType() == typeof(TConsumer));
        }
    }
}
