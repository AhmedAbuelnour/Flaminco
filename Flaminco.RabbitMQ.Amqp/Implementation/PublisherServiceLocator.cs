namespace Flaminco.RabbitMQ.AMQP.Implementation
{
    using Flaminco.RabbitMQ.AMQP.Abstractions;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Linq;

    /// <inheritdoc/>
    public class AMQPLocator(IServiceProvider serviceProvider) : IAMQPLocator
    {
        /// <inheritdoc/>
        public MessagePublisher GetPublisher<TPublisher>() where TPublisher : MessagePublisher
        {
            return serviceProvider.GetServices<MessagePublisher>().Single(a => a.GetType() == typeof(TPublisher));
        }

        /// <inheritdoc/>
        public MessageConsumer GetConsumer<TConsumer>() where TConsumer : MessageConsumer
        {
            return serviceProvider.GetServices<MessageConsumer>().Single(a => a.GetType() == typeof(TConsumer));
        }
    }
}
