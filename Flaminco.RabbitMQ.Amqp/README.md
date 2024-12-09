# Flaminco.RabbitMQ.AMQP

Flaminco.RabbitMQ.AMQP is a .NET library that simplifies the integration of RabbitMQ in your applications. This library
provides a clean and easy-to-use API for creating consumers and publishers to interact with RabbitMQ queues.

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
    options.Host = "amqp://guest:guest@localhost:5672";
    options.Username = "guest";
    options.Password = "guest";
});
```

### Step 2: Create a Message Publisher

Implement a custom publisher by extending the `MessagePublisher` class. The publisher defines the queue to which it will
send messages:

```csharp
public class PersonPublisher : MessagePublisher
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

public class Person : IMessage
{
    public string Name { get; set; }
    public int Age { get; set; }
}

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

Implement a custom consumer by extending the `MessageConsumer` class. The consumer defines the queue from which it will
receive messages:

```csharp
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
```

### Step 5: Run the Application

Build and run your application. The consumer will continuously listen for messages on the specified queue, while the
publisher sends messages to that queue.

### Step 6: Sync Message Publisher

To build synchronous communication between a publisher and waiting the consumer to return a response

```
[SyncQueueConsumer("HelloTest", typeof(ExampleRequest))]
public sealed class HelloSyncMessagePublisher(IRequestClient<ExampleRequest> requestClient) : SyncMessagePublisher<ExampleRequest>(requestClient);
```

example for using the sync publisher

```
[ApiController]
[Route("api/pdf")]
public class ExampleController(HelloSyncMessagePublisher helloSyncMessagePublisher) : ControllerBase
{
    [HttpPost("greating")]
    public async Task<IActionResult> GenerateMessage()
    {
        Response<ExampleResponse> response = await helloSyncMessagePublisher.GetResponseAsync<ExampleResponse>(new ExampleRequest
        {
            Id = 1,
        });

        return Ok(response.Message);
    }
}
```
and for consumer 

```
[QueueConsumer("HelloTest")]
public class ExampleConsumer : MessageConsumer<ExampleRequest>
{
    public override async Task Consume(ConsumeContext<ExampleRequest> context)
    {
        await context.RespondAsync<ExampleResponse>(new ExampleResponse
        {
            Message = "This is a test message"
        });
    }
}
```

Messages example

```
public class ExampleRequest : IMessage
{
    public int Id { get; set; }
}

public class ExampleResponse : IMessage
{
    public string Message { get; set; }
}
```


## Contributing

If you encounter any issues or have suggestions for improvements, please feel free to contribute by submitting an issue
or a pull request.

## License

This project is licensed under the MIT License.

