using Azure.Messaging.ServiceBus.Administration;
using Flaminco.AzureBus.AMQP.Abstractions;
using MassTransit;

namespace WebApplication1.Consumers;

public class HelloConsumer2RuleFilterProvider : IRuleFilterProvider
{
    public RuleFilter? GetRuleFilter()
    {
        return new CorrelationRuleFilter
        {
            CorrelationId = "Correlation Id Value"
        };
    }
}

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

public static class Consts
{
    public class Queues
    {
        public const string XX = "notifier-send-class-notification";
    }
}

//[QueueConsumer(queue: Consts.Queues.XX)]
//public class HelloConsumer2() : MessageConsumer<MessageBox>
//{
//    public override Task Consume(ConsumeContext<MessageBox> context)
//    {

//        return Task.CompletedTask;
//    }
//    public override Task Consume(ConsumeContext<Fault<MessageBox>> context)
//    {
//        return base.Consume(context);
//    }
//}

public class HelloPublisher(ISendEndpointProvider sendEndpointProvider)
    : MessagePublisher<MessageBox>(sendEndpointProvider)
{
    protected override string Queue => "HelloTest";

    protected override bool IsTopic => false;
}