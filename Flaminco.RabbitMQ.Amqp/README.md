# Flaminco.RabbitMQ.AMQP

Flaminco.RabbitMQ.AMQP is a .NET library that simplifies the integration of RabbitMQ AMQP 1.0 in your applications. This library provides a clean and easy-to-use API for creating consumers and publishers to interact with RabbitMQ queues.

## Installation

You can install the package via NuGet Package Manager:

```bash
dotnet add package Flaminco.RabbitMQ.AMQP
```

Or via the Package Manager Console in Visual Studio:

```powershell
Install-Package Flaminco.RabbitMQ.AMQP
```

## Getting Started

### Step 1: Configure the AMQP Client

First, you need to configure the AMQP client in your application's `Startup` or `Program` class:

```csharp
builder.Services.AddAMQPClient<Program>(options =>
{
    options.ConnectionString = "amqp://localhost:5672/";
});
```

### Step 2: Create a Message Publisher

Implement a custom publisher by extending the `MessagePublisher` class. The publisher defines the queue(s) to which it will send messages:

```csharp
public class PersonPublisher : MessagePublisher
{
    public PersonPublisher(IOptions<AddressSettings> _addressSettings) : base(_addressSettings)
    {
    }

    protected override ValueTask<string> GetKeyAsync(CancellationToken cancellationToken = default)
    {
        // A key or name for this current publisher, which is used for logs.
        return ValueTask.FromResult(nameof(PersonPublisher));
    }

    protected override ValueTask<string[]> GetQueuesAsync(CancellationToken cancellationToken = default)
    {
        // The queue name which this publisher will send the messages to.
        // This publisher can send the same message to multiple queues for different consumers.
        return ValueTask.FromResult<string[]>(new[] { "HelloQueue" });
    }
}
```

### Step 3: Send a Message

Now, you can use your custom publisher to send a message to the specified queue:

```csharp
  public class Example(IAMQPLocator _amqpLocator)
    {
        [HttpGet]
        public async Task PushMessage(CancellationToken cancellationToken)
        {
            await using MessagePublisher helloPublisher = _amqpLocator.GetPublisher<PersonPublisher>();

            await helloPublisher.PublishAsync(new Person
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
public class PersonConsumer : MessageConsumer
{
    public PersonConsumer(IOptions<AddressSettings> _addressSettings) : base(_addressSettings)
    {
    }

    protected override ValueTask<string> GetKeyAsync(CancellationToken cancellationToken = default)
    {
        // A key or name for this current consumer, which is used for logs.
        return ValueTask.FromResult(nameof(PersonConsumer));
    }

    protected override ValueTask<string> GetQueueAsync(CancellationToken cancellationToken = default)
    {
        // The queue name which this consumer will receive the messages from.
        return ValueTask.FromResult("HelloQueue");
    }
}
```

### Step 5: Implement a Hosted Service for Continuous Message Consumption

To consume messages continuously, you can implement a hosted service. This service will run in the background and process messages from the specified queue:

```csharp
public class PersonConsumerService(IAMQPLocator _amqpLocator) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // create single consumer for Person, and reuse it.
        await using MessageConsumer messageConsumer = _amqpLocator.GetConsumer<PersonConsumer>();

        Console.WriteLine("Consumer initialized successfully.");

        while (!stoppingToken.IsCancellationRequested)
        {
            Person? message = await messageConsumer.ConsumeAsync<Person>(stoppingToken);

            if (message != null)
            {
                Console.WriteLine("Consumed Message is : {0}", message.Name);
            }
        }
    }
}
```

### Step 6: Register the Hosted Service in the Dependency Injection Container

Finally, register your hosted service in the dependency injection container in your `Startup` or `Program` class:

```csharp
builder.Services.AddHostedService<PersonConsumerService>();
```

### Step 7: Run the Application

Build and run your application. The consumer will continuously listen for messages on the specified queue, while the publisher sends messages to that queue.

## Contributing

If you encounter any issues or have suggestions for improvements, please feel free to contribute by submitting an issue or a pull request.

## License

This project is licensed under the MIT License.
