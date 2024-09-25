using Flaminco.RabbitMQ.AMQP.Events;

namespace WebApplication1.HostedServices
{
    public class PersonHandler : IMessageReceivedHandler<string>, IMessageFaultHandler<string>
    {
        public async Task Handle(MessageReceivedEvent<string> notification, CancellationToken cancellationToken)
        {

            Console.WriteLine($"I got a new message saying: {notification.Message}");
        }

        public async Task Handle(MessageFaultEvent<string> notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"fault message from: {notification.Name}, and queue: {notification.Queue}");
        }
    }

    public class PersonHandler2 : IMessageReceivedHandler<string>, IMessageFaultHandler<string>
    {
        public async Task Handle(MessageReceivedEvent<string> notification, CancellationToken cancellationToken)
        {

            Console.WriteLine($"I got a new message saying 2: {notification.Message}");
        }

        public async Task Handle(MessageFaultEvent<string> notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"fault message from: {notification.Name}, and queue: {notification.Queue}");
        }
    }

    public class PersonHandler3 : IMessageReceivedHandler<string>, IMessageFaultHandler<string>
    {
        public async Task Handle(MessageReceivedEvent<string> notification, CancellationToken cancellationToken)
        {

            Console.WriteLine($"I got a new message saying 3: {notification.Message}");
        }

        public async Task Handle(MessageFaultEvent<string> notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"fault message from: {notification.Name}, and queue: {notification.Queue}");
        }
    }
}
