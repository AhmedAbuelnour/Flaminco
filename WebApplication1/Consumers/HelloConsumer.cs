using Flaminco.RabbitMQ.AMQP.Abstractions;
using MassTransit;

namespace WebApplication1.Consumers
{
    public class HelloConsumer : MessageConsumer<MessageBox>
    {
        public override string Queue => "Hellos";

        public override Task Consume(ConsumeContext<MessageBox> context)
        {
            Console.WriteLine($"I got a new message saying {context.Message.Message}");
            return Task.CompletedTask;
        }
        public override Task Consume(ConsumeContext<Fault<MessageBox>> context)
        {
            return base.Consume(context);
        }
    }

    public class MessageBox : IMessage
    {
        public string Message { get; set; }
    }
    public class HelloPublisher : MessagePublisher
    {
        public HelloPublisher(ISendEndpointProvider sendEndpointProvider) : base(sendEndpointProvider)
        {
        }

        protected override string Queue => "Hellos";
    }



}
