using Flaminco.RabbitMQ.AMQP.Abstractions;
using WebApplication1.Consumers;

namespace WebApplication1.HostedServices
{
    public class HelloHostedServices(IAMQPLocator _amqpLocator) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await using MessageConsumer messageConsumer = _amqpLocator.GetConsumer<PersonConsumer>();

            Console.WriteLine("Consumer initialized successfully.");

            while (!stoppingToken.IsCancellationRequested)
            {
                string? message = await messageConsumer.ConsumeAsync<string>(stoppingToken);

                if (message != null)
                {

                    Console.WriteLine("Consumed Message is : {0}", message);
                }
                else
                {
                    Console.WriteLine("No message received, waiting...");
                }
            }
        }
    }
}
