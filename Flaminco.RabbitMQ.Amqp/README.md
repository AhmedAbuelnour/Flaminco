# LowCodeHub.RabbitMQ.AMQP

LowCodeHub.RabbitMQ.AMQP is a .NET library that simplifies the integration of RabbitMQ with AMQP protocol in your applications. This library provides a clean and easy-to-use API for creating consumers and publishers to interact with RabbitMQ queues.

## Features

- Simple abstractions for message publishing and consuming
- Automatic queue declaration and management
- Support for message properties and customization options
- Built-in health checks for RabbitMQ connections
- Automatic registration of publishers and consumers
- Graceful handling of connection failures and retries
- Support for .NET 9.0

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
// Configure using environment variables or your preferred configuration approach
string rabbitMQHost = Environment.GetEnvironmentVariable("RabbitMQHost");
string rabbitMQVHost = Environment.GetEnvironmentVariable("RabbitMQVHost");
string rabbitMQUsername = Environment.GetEnvironmentVariable("RabbitMQUsername");
string rabbitMQPassword = Environment.GetEnvironmentVariable("RabbitMQPassword");

// Add RabbitMQ services to your application
services.AddAmqpClient(options =>
{
    options.HostName = rabbitMQHost;
    options.VirtualHost = rabbitMQVHost;
    options.UserName = rabbitMQUsername;
    options.Password = rabbitMQPassword;
    options.ClientProvidedName = Environment.GetEnvironmentVariable("ApplicationId") ?? "MyApp";
}, typeof(Program).Assembly); // Specify the assembly to scan for publishers and consumers
```

The library will automatically:
- Register the AMQP connection provider
- Scan the specified assembly for publisher and consumer implementations
- Register health checks for the RabbitMQ connection

### Step 2: Create a Message Publisher

Implement a custom publisher by extending the `MessagePublisher` class. The publisher defines the queue to which it will send messages:

```csharp
public class PersonPublisher : MessagePublisher
{
    public PersonPublisher(AmqpConnectionProvider connectionProvider) : base(connectionProvider)
    {
    }

    protected override string Queue => "HelloQueue";
}
```

### Step 3: Send a Message

Now, you can use your custom publisher to send a message to the specified queue:

```csharp
// Define your message class (must be serializable)
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}

// Use the publisher in your service or controller
public class PersonService
{
    private readonly PersonPublisher _personPublisher;
    
    public PersonService(PersonPublisher personPublisher)
    {
        _personPublisher = personPublisher;
    }
    
    public async Task CreatePersonAsync(Person person, CancellationToken cancellationToken)
    {
        // Simple message publishing
        await _personPublisher.PublishAsync(person, cancellationToken);
        
        // Publishing with options
        var properties = new BasicProperties
        {
            MessageId = Guid.NewGuid().ToString(),
            CorrelationId = Guid.NewGuid().ToString(),
            ContentType = "application/json",
            Expiration = "3600000", // TTL in milliseconds (1 hour)
            Headers = new Dictionary<string, object>
            {
                ["Priority"] = "High"
            }
        };
        
        await _personPublisher.PublishAsync(person, properties, cancellationToken);
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
    
    public override Task ConsumeAsync(Person message, IReadOnlyBasicProperties properties, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received message: {Name}, Age: {Age}", message.Name, message.Age);
        
        // You can access message properties
        if (properties.Headers != null && properties.Headers.TryGetValue("Priority", out var priority))
        {
            _logger.LogInformation("Message priority: {Priority}", priority);
        }
        
        return Task.CompletedTask;
    }

    public override Task ConsumeAsync(Exception error, IReadOnlyBasicProperties properties, CancellationToken cancellationToken)
    {
        _logger.LogError(error, "Error processing message");
        return Task.CompletedTask;
    }
}
```

### Step 5: Run the Application

Build and run your application. The hosted service will automatically:

1. Discover all consumers decorated with the `QueueConsumerAttribute`
2. Create channels for each queue
3. Ensure the queues exist in RabbitMQ
4. Set up consumers to listen for messages

Publishers will be registered as scoped services and will be available for dependency injection wherever needed.

## Advanced Features

### Queue Auto-Creation

The library automatically creates queues as needed:

- When publishers send messages to a queue that doesn't exist, the queue is automatically created
- When consumers attempt to consume from a queue that doesn't exist, the queue is automatically created

This behavior ensures that your application can start in any order without requiring manual queue configuration.

### Health Checks

The library automatically registers a health check for the RabbitMQ connection. You can use this with ASP.NET Core's health check middleware:

```csharp
app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

### Connection Management

The `AmqpConnectionProvider` service manages RabbitMQ connections and channels:

- Connections are created on-demand and cached
- Channels are cached for each queue
- Connections and channels are automatically recovered after failures
- Resources are properly disposed when the application shuts down

## Contributing

If you encounter any issues or have suggestions for improvements, please feel free to contribute by submitting an issue or a pull request.

## License

This project is licensed under the MIT License.

