# LowCodeHub.RabbitMQ.AMQP

LowCodeHub.RabbitMQ.AMQP is a .NET library that simplifies the integration of RabbitMQ with AMQP protocol in your applications. This library provides a clean and easy-to-use API for creating consumers and publishers to interact with RabbitMQ queues.

## Features

- Simple abstractions for message publishing and consuming
- Automatic queue declaration and management
- Support for message properties and customization options
- Built-in health checks for RabbitMQ connections
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

### Step 1: Configure MassTransit

First, you need to configure the AMQP client in your application's `Startup` or `Program` class:

```csharp
// Configure using environment variables or your preferred configuration approach
string rabbitMQHost = Environment.GetEnvironmentVariable("RabbitMQHost");
string rabbitMQVHost = Environment.GetEnvironmentVariable("RabbitMQVHost");
string rabbitMQUsername = Environment.GetEnvironmentVariable("RabbitMQUsername");
string rabbitMQPassword = Environment.GetEnvironmentVariable("RabbitMQPassword");

// Register MassTransit with RabbitMQ
services.AddAmqpClient(
    register: x =>
    {
        x.AddConsumer<PersonConsumer>();
        x.AddScoped<PersonPublisher>();
    },
    busConfiguration: (cfg, context) =>
    {
        cfg.Host(rabbitMQHost, rabbitMQVHost, h =>
        {
            h.Username(rabbitMQUsername);
            h.Password(rabbitMQPassword);
        });

        cfg.ReceiveEndpoint(Constant.Queues.PersonCreated, e =>
        {
            e.ConfigureConsumer<PersonConsumer>(context);
        });

        cfg.UseRawJsonSerializer();
    });
```

The library registers MassTransit with RabbitMQ and adds a health check for the connection.

### Step 2: Create a Message Publisher

Implement a custom publisher by extending the `MessagePublisher` class. The publisher defines the queue to which it will send messages:

```csharp
public class PersonPublisher : MessagePublisher
{
    public PersonPublisher(ISendEndpointProvider sendProvider, IPublishEndpoint publishEndpoint) : base(sendProvider, publishEndpoint)
    {
    }

    // Use queue names from your constants
    protected override string Queue => Constant.Queues.PersonCreated;
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
        await _personPublisher.PublishAsync(person, cancellationToken);
    }
}
```

### Step 4: Create a Message Consumer

Implement a custom consumer by extending the `MessageConsumer` class. You will configure the queue when registering the consumer:

```csharp
public class PersonConsumer : MessageConsumer<Person>
{
    private readonly ILogger<PersonConsumer> _logger;

    public PersonConsumer(ILogger<PersonConsumer> logger)
    {
        _logger = logger;
    }

    public override Task Consume(ConsumeContext<Person> context)
    {
        _logger.LogInformation("Received message: {Name}, Age: {Age}", context.Message.Name, context.Message.Age);
        return Task.CompletedTask;
    }
}
```

### Step 5: Run the Application

Build and run your application. MassTransit will automatically:

1. Create the required queues if they do not exist
2. Configure retries and move failed messages to a queue with the `_error` suffix

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

MassTransit manages the RabbitMQ connections and automatically creates and recovers channels when required.

## Contributing

If you encounter any issues or have suggestions for improvements, please feel free to contribute by submitting an issue or a pull request.

## License

This project is licensed under the MIT License.

