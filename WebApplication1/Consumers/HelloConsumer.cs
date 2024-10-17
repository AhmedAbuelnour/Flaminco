﻿using Azure.Messaging.ServiceBus.Administration;
using Flaminco.AzureBus.AMQP.Abstractions;
using Flaminco.AzureBus.AMQP.Attributes;
using MassTransit;

namespace WebApplication1.Consumers
{

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

    [TopicConsumer(topic: "hello", subscription: "HelloConsumer2", ruleFilterType: typeof(HelloConsumer2RuleFilterProvider))]
    public class HelloConsumer2(IServiceProvider serviceProvider, ApplicationDbContext dbContext) : MessageConsumer<MessageBox>
    {
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
    public class HelloPublisher(ISendEndpointProvider sendEndpointProvider) : MessagePublisher(sendEndpointProvider)
    {
        protected override string Queue => "hello";

        protected override bool IsTopic => true;
    }
}
