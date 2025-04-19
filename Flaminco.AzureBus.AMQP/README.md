# LowCodeHub.RabbitMQ.AMQP

LowCodeHub.RabbitMQ.AMQP is a .NET library that simplifies the integration of RabbitMQ with AMQP 1.0 protocol in your applications. This library provides a clean and easy-to-use API for creating consumers and publishers to interact with RabbitMQ queues using AMQP 1.0.

## Installation

You can install the package via NuGet Package Manager:

```bash
dotnet add package LowCodeHub.RabbitMQ.AMQP
```

Or via the Package Manager Console in Visual Studio:

```powershell
Install-Package LowCodeHub.RabbitMQ.AMQP
```

## Getting Started

### Step 1: Configure the AMQP Client

First, you need to configure the AMQP client in your application's `Startup` or `Program` class:

```csharp
builder.Services.AddAMQPClient<Program>(options =>
{
    options.Host = "amqp://localhost:5672"; // RabbitMQ host with AMQP 1.0 plugin enabled
    options.Username = "guest";
    options.Password = "guest";
    options.RetryCount = 3; // Optional: Set retry count
    options.RetryInterval = TimeSpan.FromSeconds(2); // Optional: Set retry interval
});
```

> **Note**: To use AMQP 1.0 with RabbitMQ 4, you need to install the RabbitMQ AMQP 1.0 plugin:
> ```bash
> rabbitmq-plugins enable rabbitmq_amqp1_0
> ```

### Step 2: Create a Message Publisher

Implement a custom publisher by extending the `MessagePublisher` class. The publisher defines the queue to which it will send messages:

```csharp
public class PersonPublisher : MessagePublisher
{
    public PersonPublisher(AMQPConnectionProvider connectionProvider) : base(connectionProvider)
    {
    }

    protected override string Queue => "HelloQueue";
}
```

### Step 3: Send a Message

Now, you can use your custom publisher to send a message to the specified queue:

```csharp
public class Person : IMessage
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public class Example(PersonPublisher personPublisher)
{
    [HttpGet]
    public async Task PushMessage(CancellationToken cancellationToken)
    {
        // Simple message publishing
        await personPublisher.PublishAsync(new Person
        {
            Name = "Ahmed Abuelnour",
            Age = 30
        }, cancellationToken);
        
        // Publishing with options
        var options = new MessagePublishOptions
        {
            MessageId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid(),
            TimeToLive = TimeSpan.FromHours(1),
            ContentType = "application/json",
            ApplicationProperties = new Dictionary<string, string>
            {
                ["Priority"] = "High"
            }
        };
        
        await personPublisher.PublishAsync(new Person
        {
            Name = "John Doe",
            Age = 25
        }, options, cancellationToken);
    }
}
```

### Step 4: Create a Message Consumer

Implement a custom consumer by extending the `MessageConsumer` class. The consumer defines the queue from which it will receive messages using the `QueueConsumerAttribute`:

```csharp
[QueueConsumer(queue: "HelloQueue")]
public class PersonConsumer : MessageConsumer<Person>
{
    private readonly ILogger<PersonConsumer> _logger;
    
    public PersonConsumer(ILogger<PersonConsumer> logger)
    {
        _logger = logger;
    }
    
    public override Task ConsumeAsync(Person message, MessageProperties properties, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received message: {Name}, Age: {Age}", message.Name, message.Age);
        
        // You can access message properties
        if (properties.ApplicationProperties.TryGetValue("Priority", out var priority))
        {
            _logger.LogInformation("Message priority: {Priority}", priority);
        }
        
        return Task.CompletedTask;
    }

    public override Task ConsumeErrorAsync(Exception error, MessageProperties properties, CancellationToken cancellationToken)
    {
        _logger.LogError(error, "Error processing message");
        return Task.CompletedTask;
    }
}
```

### Step 5: Run the Application

Build and run your application. The consumer will automatically start and listen for messages on the specified queue, while the publisher can be used to send messages to that queue.

## Contributing

If you encounter any issues or have suggestions for improvements, please feel free to contribute by submitting an issue or a pull request.

## License

This project is licensed under the MIT License.

