namespace Flaminco.AzureBus.AMQP.Implementation
{
    using Flaminco.AzureBus.AMQP.Abstractions;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Linq;

    /// <inheritdoc/>
    public class AMQPLocator(IServiceProvider serviceProvider) : IAMQPLocator
    {
        /// <inheritdoc/>
        public MessagePublisher GetPublisher<TPublisher>() where TPublisher : MessagePublisher
        {
            IServiceScope scope = serviceProvider.CreateScope();

            return scope.ServiceProvider.GetServices<MessagePublisher>().Single(a => a.GetType() == typeof(TPublisher));
        }

        /// <inheritdoc/>
        public MessageConsumer GetConsumer<TConsumer>() where TConsumer : MessageConsumer
        {
            IServiceScope scope = serviceProvider.CreateScope();

            return scope.ServiceProvider.GetServices<MessageConsumer>().Single(a => a.GetType() == typeof(TConsumer));
        }
    }
}