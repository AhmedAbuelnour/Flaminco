using Flaminco.AzureBus.AMQP.Attributes;
using Flaminco.Contracts;
using Flaminco.RabbitMQ.AMQP.Abstractions;
using MassTransit;

namespace WebApplication1.Consumers;

//[QueueConsumer(queue: "hello")]

//public class HelloConsumer(IServiceProvider serviceProvider, ApplicationDbContext dbContext) : MessageConsumer<MessageBox>
//{
//    public override Task Consume(ConsumeContext<MessageBox> context)
//    {
//        Console.WriteLine($"I got a new message saying {context.Message.Message}");

//        return Task.CompletedTask;
//    }
//    public override Task Consume(ConsumeContext<Fault<MessageBox>> context)
//    {
//        return base.Consume(context);
//    }
//}

[MessageFlow("HelloTest", typeof(ExampleRequest))]
public sealed class HelloMessageFlow(IRequestClient<ExampleRequest> requestClient) : MessageFlow<ExampleRequest>(requestClient);
