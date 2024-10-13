namespace Flaminco.AzureBus.AMQP.Abstractions
{
    using Microsoft.Extensions.Hosting;
    using System.Threading;

    internal class AMQPBackgroundService<TConsumer, TMessage>(IAMQPLocator _amqpLocator) : BackgroundService where TConsumer : MessageConsumer where TMessage : notnull
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await using MessageConsumer messageConsumer = _amqpLocator.GetConsumer<TConsumer>();

            await messageConsumer.ConsumeAsync<TMessage>(stoppingToken);
        }
    }

}
