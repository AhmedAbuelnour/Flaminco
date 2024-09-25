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
    options.ConnectionString = "amqp://guest:guest@localhost:5672";
});
```

### Step 2: Create a Message Publisher

Implement a custom publisher by extending the `MessagePublisher` class. The publisher defines the queue(s) to which it will send messages:

```csharp
    public class PersonPublisher : MessagePublisher
    {
        public PersonPublisher(IOptions<AMQPClientSettings> clientSettings) : base(clientSettings)
        {
        }

        protected override string Name => nameof(PersonPublisher);
        protected override string[] Queues => ["HelloQueue"];
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
        public PersonConsumer(IOptions<AMQPClientSettings> clientSettings, IPublisher publisher) : base(clientSettings, publisher)
        {
        }

        protected override string Name => nameof(PersonConsumer);
        protected override string Queue => "HelloQueue";
    }
```

### Step 5: Implement a message handler

IMessageFaultHandler is Optional to handle the cases where the consumer couldn't deal with the incoming message, and of course you can have multiple handlers for the same message.

```csharp

    public class PersonMessageHandler : IMessageReceivedHandler<Person>, IMessageFaultHandler<Person>
    {
        public async Task Handle(MessageReceivedEvent<Person> notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"I got a new message saying: {notification.Message}");
        }

        public async Task Handle(MessageFaultEvent<Person> notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"fault message from: {notification.Name}, and queue: {notification.Queue}");
        }
    }
```

### Step 6: Register the consumers to be marked as background service

Finally, register your consumers in the dependency injection container in your `Startup` or `Program` class:

```csharp
    builder.Services.AddAMQPService<PersonConsumer,Person>();
```


### Step 7: Run the Application

Build and run your application. The consumer will continuously listen for messages on the specified queue, while the publisher sends messages to that queue.

## Contributing

If you encounter any issues or have suggestions for improvements, please feel free to contribute by submitting an issue or a pull request.

## License

This project is licensed under the MIT License.
