# Flaminco.AzureBus.AMQP

Flaminco.AzureBus.AMQP is a .NET library that simplifies the integration of Azure Bus in your applications. This library provides a clean and easy-to-use API for creating consumers and publishers to interact with AzureBus queues.

## Installation

You can install the package via NuGet Package Manager:

```bash
dotnet add package Flaminco.AzureBus.AMQP
```

Or via the Package Manager Console in Visual Studio:

```powershell
Install-Package Flaminco.AzureBus.AMQP
```

## Getting Started

### Step 1: Configure the AMQP Client

First, you need to configure the AMQP client in your application's `Startup` or `Program` class:

```csharp
builder.Services.AddAMQPClient<Program>(options =>
{
    options.ConnectionString = "<Azure Bus Connection String>";
});
```

### Step 2: Create a Message Publisher

Implement a custom publisher by extending the `MessagePublisher` class. The publisher defines the queue(s) to which it will send messages:

```csharp

// must be shared between your consumer and publisher (Must be same type not only identical in properties)
public class Person : IMessage
{
    public string Name { get; set; }
    public int Age { get; set; }
}


public class PersonPublisher : MessagePublisher<Person>
{
    public PersonPublisher(ISendEndpointProvider sendEndpointProvider) : base(sendEndpointProvider)
    {
    }

    protected override string Queue => "HelloQueue";
}
```

### Step 3: Send a Message

Now, you can use your custom publisher to send a message to the specified queue:

```csharp
public class Example(PersonPublisher _personPublisher)
{
    [HttpGet]
    public async Task PushMessage(CancellationToken cancellationToken)
    {
        await _personPublisher.PublishAsync(new Person
        {
            Name = "Ahmed Abuelnour",
            Age = 30
        }, cancellationToken);
    }
}
```

### Step 4: Create a Message Consumer

Implement a custom consumer by extending the `MessageConsumer` class. The consumer defines the queue from which it will receive messages:

```csharp

// For queue consumers

[QueueConsumer(queue: "HelloQueue")]
public class PersonConsumer : MessageConsumer<Person>
{
    public override Task Consume(ConsumeContext<Person> context)
    {
        Console.WriteLine($"Received message: {context.Message.Name}, Age: {context.Message.Age}");
        return Task.CompletedTask;
    }

   public override Task Consume(ConsumeContext<Fault<MessageBox>> context)
   {
       return base.Consume(context);
   }
}

// For topic consumers

[TopicConsumer(topic: "HelloQueue", subscription: nameof(PersonConsumer))]
public class PersonConsumer : MessageConsumer<Person>
{
    public override Task Consume(ConsumeContext<Person> context)
    {
        Console.WriteLine($"Received message: {context.Message.Name}, Age: {context.Message.Age}");
        return Task.CompletedTask;
    }

   public override Task Consume(ConsumeContext<Fault<MessageBox>> context)
   {
       return base.Consume(context);
   }
}

// For filter based topic consumer

// you can define sql filter
public class SqlFilterProvider : IRuleFilterProvider
{
    public RuleFilter? GetRuleFilter()
    {
       // Filter key is the one you use in MessagePublishOptions, and Filter Value is the value you pass
       return new SqlRuleFilter("FilterKey = FilterValue");
    }
}

// or you can define correlation filter

public class CorrelationFilterProvider : IRuleFilterProvider
{
    public RuleFilter? GetRuleFilter()
    {
        return new CorrelationRuleFilter
        {
            CorrelationId = "Correlation Id Value"
        };
    }
}

[TopicConsumer(topic: "HelloQueue", subscription: nameof(PersonConsumer), typeof(CorrelationFilterProvider))]
public class PersonConsumer : MessageConsumer<Person>
{
    public override Task Consume(ConsumeContext<Person> context)
    {
        Console.WriteLine($"Received message: {context.Message.Name}, Age: {context.Message.Age}");
        return Task.CompletedTask;
    }

   public override Task Consume(ConsumeContext<Fault<MessageBox>> context)
   {
       return base.Consume(context);
   }
}
```

### Step 5: Run the Application

Build and run your application. The consumer will continuously listen for messages on the specified queue, while the publisher sends messages to that queue.

## Contributing

If you encounter any issues or have suggestions for improvements, please feel free to contribute by submitting an issue or a pull request.

## License

This project is licensed under the MIT License.

