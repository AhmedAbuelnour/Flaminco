using Microsoft.Extensions.Hosting;

namespace Flaminco.RabbitMQ.AMQP.Abstractions
{
    internal class AMQPBackgroundService<TConsumer, TMessage>(IAMQPLocator _amqpLocator) : BackgroundService where TConsumer : MessageConsumer where TMessage : notnull
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await using MessageConsumer messageConsumer = _amqpLocator.GetConsumer<TConsumer>();

            while (!stoppingToken.IsCancellationRequested)
            {
                await messageConsumer.ConsumeAsync<TMessage>(stoppingToken);
            }
        }
    }

}
