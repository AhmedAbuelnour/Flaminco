using Flaminco.RabbitMQ.AMQP.Events;

namespace WebApplication1.HostedServices
{
    //public class HelloHostedServices(IAMQPLocator _amqpLocator) : BackgroundService
    //{
    //    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    //    {
    //        await using MessageConsumer messageConsumer = _amqpLocator.GetConsumer<HelloConsumer>();

    //        Console.WriteLine("Consumer initialized successfully.");

    //        while (!stoppingToken.IsCancellationRequested)
    //        {
    //            await messageConsumer.ConsumeAsync<string>(stoppingToken);
    //        }
    //    }
    //}


    public class PersonHandler : IMessageReceivedHandler<string>, IMessageFaultHandler
    {
        public async Task Handle(MessageReceivedEvent<string> notification, CancellationToken cancellationToken)
        {

            Console.WriteLine($"I got a new message saying: {notification.Message}");
        }

        public async Task Handle(MessageFaultEvent notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"fault message from: {notification.Name}, and queue: {notification.Queue}");
        }
    }

    public class PersonHandler2 : IMessageReceivedHandler<string>, IMessageFaultHandler
    {
        public async Task Handle(MessageReceivedEvent<string> notification, CancellationToken cancellationToken)
        {

            Console.WriteLine($"I got a new message saying 2: {notification.Message}");
        }

        public async Task Handle(MessageFaultEvent notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"fault message from: {notification.Name}, and queue: {notification.Queue}");
        }
    }

    public class PersonHandler3 : IMessageReceivedHandler<string>, IMessageFaultHandler
    {
        public async Task Handle(MessageReceivedEvent<string> notification, CancellationToken cancellationToken)
        {

            Console.WriteLine($"I got a new message saying 3: {notification.Message}");
        }

        public async Task Handle(MessageFaultEvent notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"fault message from: {notification.Name}, and queue: {notification.Queue}");
        }
    }
}
