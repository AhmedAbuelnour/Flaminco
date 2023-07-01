

# How to use it?

You need to inject the service locator into the DI system, which must be located in the same library that your services are located at. 

```csharp

services.AddBusLocator(configuration).AddPublishers<IPublisherScanner>().AddConsumers<IConsumerScanner>();

```

And provide the service bus connection string.

```json
"ServiceBusSettings": {
    "ConnectionString": "<Your-Connection-String>"
}
```

We have 2 service types for publishers
* MessagePublisher for regular messages.
* ScheduleMessagePublisher for schedule messages.

### Example for implementing a publisher 

```csharp
public class NotificationPublisher : MessagePublisher
{
    public NotificationPublisher(IOptions<ServiceBusSettings> options) : base(options)
    {
    }

    public override string QueueOrTopicName { get; set; } = "notifier";
}
``` 

We have 2 service types for consumers
* MessageQueueConsumer  for queue consumers.
* MessageTopicConsumer for topic consumers.

###  Example for implementing a topic consumer

```csharp
 public class NotificationConsumer : MessageTopicConsumer
 {
    public NotificationConsumer(IOptions<ServiceBusSettings> options) : base(options)
    {
    }

    public override string TopicName { get; init; } = "notifier";
    public override string SubscriptionName { get; init; } = "notifications";
}
``` 

## How to start working with each of them?

###  Example for using publisher.
 
```csharp
 public async Task<string> Get([FromServices] IServiceBusLocator _serviceBusLocator)
 {
    MessagePublisher? notificationPublisher = _serviceBusLocator.GetPublisher<NotificationPublisher>();

    await notificationPublisher?.Publish(new OrderCreated { Id = 1, Name = "Test" });

    return "ok";
}
```

###  Example for using consumer in a hosted service.
```csharp
 public class NotificationConsumerService : IHostedService
    {
        private readonly MessageTopicConsumer? _messageTopicConsumer;
        public NotificationConsumerService(IServiceBusLocator serviceBusLocator)
        {
            _messageTopicConsumer = serviceBusLocator.GetTopicConsumer<NotificationConsumer>();
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {

            return _messageTopicConsumer.Consume(async (message) =>
             {
                 Notification? notification = message.Message.Body.ToObjectFromJson<Notification>();

                 // handle notification here...

                 await message.CompleteMessageAsync(message.Message);

             }, async (error) =>
             {
                 // handle error here
             }, cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _messageTopicConsumer.Dispose();
        }
    }
}
```
