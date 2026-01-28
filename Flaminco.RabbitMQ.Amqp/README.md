# Flaminco.RabbitMQ.AMQP

A powerful, modern .NET library that provides a clean abstraction layer over RabbitMQ.Client 7.2 for ASP.NET Core applications targeting .NET 10.

## Features

- **Modern Async API** - Built entirely on async/await patterns
- **Automatic Connection Recovery** - Built-in resilience with automatic reconnection
- **Publisher Confirms** - Reliable message delivery with broker confirmations
- **Dead Letter Queue Support** - Automatic DLQ creation and routing
- **Fluent Topology API** - Clean, hierarchical topology configuration in code
- **Pluggable Serialization** - Use JSON (default) or implement custom serializers
- **Health Checks** - Built-in health check for connection monitoring

## Installation

```shell
dotnet add package Flaminco.RabbitMQ.AMQP
```
```
Exchange   : {domain}.exchange 
Routing key: {entity}.{event}
Queue      : {service}.{entity}.{event}.queue
```
---

## Quick Start

```csharp
// Program.cs
using Flaminco.RabbitMQ.AMQP.Extensions;
using Flaminco.RabbitMQ.AMQP.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRabbitMq(
    options =>
    {
        options.HostName = "localhost";
        options.Port = 5672;
        options.UserName = "guest";
        options.Password = "guest";
    },
    topology =>
    {
        // Define your topology here (see below)
    });

var app = builder.Build();
app.Run();
```

---

## Topology Configuration (Fluent API)

### Basic Structure

Exchanges contain their queues, bindings, and dead-letter config - all in one place:

```csharp
builder.Services.AddRabbitMq(
    options => { /* connection options */ },
    topology =>
    {
        // Exchange with queues and bindings
        topology.Exchange("orders", exchange => exchange
            .Topic()
            .WithDeadLetterExchange()  // Auto-creates "orders.dlx"
            .Queue("orders.created", "order.created.#", queue => queue
                .DeadLetter())  // Auto-creates "orders.created.dlq"
            .Queue("orders.shipped", "order.shipped.#", queue => queue
                .MaxLength(10000)
                .DeadLetter(dl => dl.Queue("orders.shipped.failed"))));

        // Another exchange
        topology.Exchange("notifications", exchange => exchange
            .Fanout()
            .Queue("notifications.email", "")
            .Queue("notifications.sms", ""));

        // Standalone queue (uses default exchange)
        topology.Queue("background.jobs", queue => queue
            .MaxLength(10000)
            .DeadLetter());
    });
```

### What Gets Created

```
Exchange "orders" (topic)
├── Exchange "orders.dlx" (direct) - auto-created
├── Queue "orders.created"
│   ├── Binding: "order.created.#"
│   └── DLQ: "orders.created.dlq" → bound to "orders.dlx"
└── Queue "orders.shipped"
    ├── Binding: "order.shipped.#"
    ├── MaxLength: 10000
    └── DLQ: "orders.shipped.failed" → bound to "orders.dlx"

Exchange "notifications" (fanout)
├── Queue "notifications.email"
└── Queue "notifications.sms"

Queue "background.jobs" (default exchange)
└── DLQ: "background.jobs.dlq"
```

---

## Complete Fluent API Reference

### Exchange Configuration

```csharp
topology.Exchange("name", exchange => exchange
    .Topic()                           // or .Direct(), .Fanout(), .Headers()
    .Durable(true)                     // default: true
    .WithDeadLetterExchange("custom")  // optional: custom DLX name (default: {name}.dlx)
    .Queue("queue.name", "routing.key", queue => ...));
```

### Queue Configuration (Bound to Exchange)

```csharp
.Queue("name", "routing.key", queue => queue
    .Durable(true)                     // default: true
    .MessageTtl(60000)                 // TTL in milliseconds
    .MaxLength(10000)                  // max messages
    .MaxLengthBytes(104857600)          // max bytes
    .MaxPriority(10)                   // enable priority queue (0-255)
    .QueueMode("lazy")                 // lazy queue
    .DeadLetter(dl => dl               // configure dead-letter
        .Exchange("custom.dlx")        // optional: custom DLX name
        .Queue("custom.dlq")           // optional: custom DLQ name
        .RoutingKey("custom.key")      // optional: custom routing key
        .MessageTtl(30000)));          // optional: DLQ message TTL
```

### Standalone Queue Configuration

```csharp
topology.Queue("name", queue => queue
    .Durable(true)
    .MessageTtl(60000)
    .MaxLength(10000)
    .DeadLetter("custom.dlq"));       // optional: custom DLQ name
```

---

## Consuming Messages

**Consumers only need the queue name!** The `[Queue]` attribute is **required** for each consumer.

```csharp
using Flaminco.RabbitMQ.AMQP.Abstractions;
using Flaminco.RabbitMQ.AMQP.Attributes;

// Required: [Queue] attribute with queue name
[Queue("orders.created", PrefetchCount = 20, MaxRetryAttempts = 3)]
public class OrderCreatedConsumer : MessageConsumer<OrderCreatedEvent>
{
    private readonly IOrderService _orderService;

    public OrderCreatedConsumer(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public override async Task ConsumeAsync(
        ConsumeContext<OrderCreatedEvent> context,
        CancellationToken cancellationToken = default)
    {
        await _orderService.ProcessAsync(context.Message, cancellationToken);
    }
}
```

### Consumer with Lifecycle Hooks

```csharp
[Queue("orders.processor", MaxRetryAttempts = 3)]
public class OrderProcessor : MessageConsumer<OrderMessage>
{
    public override async Task ConsumeAsync(
        ConsumeContext<OrderMessage> context,
        CancellationToken cancellationToken = default)
    {
        // Access message and metadata
        var order = context.Message;
        var messageId = context.MessageId;
        var correlationId = context.CorrelationId;
        var retryCount = context.RetryCount;
        var customHeader = context.GetHeaderString("x-source");
        
        await ProcessOrderAsync(order);
    }

    // Pre-processing - return false to skip
    public override Task<bool> OnBeforeConsumeAsync(
        ConsumeContext<OrderMessage> context,
        CancellationToken cancellationToken = default)
    {
        if (context.Message.OrderId == Guid.Empty)
            return Task.FromResult(false); // Skip and acknowledge
        
        return Task.FromResult(true);
    }

    // Post-processing
    public override Task OnAfterConsumeAsync(
        ConsumeContext<OrderMessage> context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processed {OrderId}", context.Message.OrderId);
        return Task.CompletedTask;
    }

    // Error handling
    public override Task<ErrorHandlingResult> OnErrorAsync(
        ConsumeContext<OrderMessage> context,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        if (context.RetryCount < 3)
            return Task.FromResult(ErrorHandlingResult.Requeue);
        
        return Task.FromResult(ErrorHandlingResult.Reject); // Goes to DLQ
    }
}
```

### QueueAttribute Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Name` | string | **required** | Queue name to consume from |
| `PrefetchCount` | ushort | 0 (uses global) | Messages to prefetch |
| `MaxRetryAttempts` | int | 3 | Max retries before DLQ |
| `RequeueOnFailure` | bool | false | Requeue on error |
| `Durable` | bool | true | Queue durability |
| `DeadLetterExchange` | string | null | DLX name |
| `DeadLetterRoutingKey` | string | null | DLQ routing key |
| `MessageTtl` | int | 0 | Message TTL in ms |

---

## Publishing Messages

Inject `IMessagePublisher`:

```csharp
using Flaminco.RabbitMQ.AMQP.Abstractions;

public class OrderService
{
    private readonly IMessagePublisher _publisher;

    public OrderService(IMessagePublisher publisher)
    {
        _publisher = publisher;
    }

    // Publish to queue (default exchange)
    public Task CreateOrderAsync(Order order) =>
        _publisher.PublishToQueueAsync("orders.created", order);

    // Publish to exchange with routing key
    public Task NotifyAsync(OrderEvent evt) =>
        _publisher.PublishAsync("orders", "order.created.new", evt);

    // Reliable publish with broker confirmation
    public Task<bool> CriticalPublishAsync(Order order) =>
        _publisher.PublishWithConfirmAsync("orders", "order.critical", order);

    // With options
    public Task PublishWithOptionsAsync(Order order) =>
        _publisher.PublishAsync("orders", "order.priority", order, new PublishOptions
        {
            Priority = 9,
            CorrelationId = order.Id.ToString(),
            Headers = { ["source"] = "api" }
        });

    // Batch publish
    public Task BatchPublishAsync(IEnumerable<Order> orders) =>
        _publisher.PublishBatchAsync("orders", "order.batch", orders);
}
```

---

## Routing Key Patterns (Topic Exchange)

| Pattern | Matches | Example |
|---------|---------|---------|
| `order.created.#` | Zero or more words after | `order.created`, `order.created.new`, `order.created.new.customer` |
| `order.*.status` | Exactly one word | `order.new.status`, `order.old.status` |
| `order.#` | Any order event | `order.created`, `order.updated`, `order.deleted` |
| `#` | Everything | All routing keys |

---

## Configuration Options

### Connection Options

```csharp
builder.Services.AddRabbitMq(
    options =>
    {
        options.HostName = "localhost";
        options.Port = 5672;
        options.VirtualHost = "/";
        options.UserName = "guest";
        options.Password = "guest";
        options.AutomaticRecoveryEnabled = true;
        options.NetworkRecoveryIntervalSeconds = 5;
        options.RequestedHeartbeat = 60;
        options.DefaultPrefetchCount = 10;
    },
    topology => { /* topology config */ });
```

### SSL/TLS

```csharp
options.Ssl = new RabbitMqSslOptions
{
    Enabled = true,
    ServerName = "rabbitmq.example.com",
    CertificatePath = "/certs/client.pfx",
    CertificatePassword = "secret"
};
```

### Clustering

```csharp
options.Endpoints = new List<RabbitMqEndpoint>
{
    new() { HostName = "rabbit1.example.com", Port = 5672 },
    new() { HostName = "rabbit2.example.com", Port = 5672 }
};
```

---

## Custom Serialization

```csharp
public class MessagePackSerializer : IMessageSerializer
{
    public string ContentType => "application/x-msgpack";
    public byte[] Serialize<T>(T message) => MessagePackSerializer.Serialize(message);
    public T? Deserialize<T>(ReadOnlySpan<byte> data) => MessagePackSerializer.Deserialize<T>(data.ToArray());
    public object? Deserialize(ReadOnlySpan<byte> data, Type type) => MessagePackSerializer.Deserialize(type, data.ToArray());
}

builder.Services.AddRabbitMq(options, topology)
    .UseMessageSerializer<MessagePackSerializer>();
```

---

## Summary

### Topology Setup (Fluent API Only)

```csharp
topology.Exchange("orders", e => e
    .Topic()
    .WithDeadLetterExchange()
    .Queue("orders.created", "order.created.#", q => q.DeadLetter())
    .Queue("orders.shipped", "order.shipped.#", q => q.MaxLength(10000)));
```

### Consumer Setup (Queue Attribute Required)

```csharp
[Queue("orders.created")]  // Required: queue name
public class OrderConsumer : MessageConsumer<OrderEvent>
{
    public override Task ConsumeAsync(ConsumeContext<OrderEvent> context, CancellationToken ct)
    {
        // Process message
        return Task.CompletedTask;
    }
}
```

### Publishing

```csharp
await _publisher.PublishAsync("orders", "order.created.new", message);
```

---

## RPC (Request-Reply Pattern)

The library provides full RPC support for synchronous request-reply communication over RabbitMQ.

### Enable RPC

```csharp
builder.Services.AddRabbitMq(builder.Configuration, topology =>
{
    // Define RPC queue
    topology.Exchange("rpc.exchange", exchange => exchange.Direct()
        .Queue("calculator.queue", "calculator"));
});

// Enable RPC client
builder.Services.AddRabbitMqRpc();
```

### RPC Server (Handling Requests)

```csharp
public record CalculateRequest(int A, int B, string Operation);
public record CalculateResponse(int Result, string Message);

[Queue("calculator.queue")]
public class CalculatorRpcConsumer : RpcMessageConsumer<CalculateRequest, CalculateResponse>
{
    private readonly ILogger<CalculatorRpcConsumer> _logger;

    public CalculatorRpcConsumer(ILogger<CalculatorRpcConsumer> logger)
    {
        _logger = logger;
    }

    public override async Task<CalculateResponse> HandleAsync(
        RpcContext<CalculateRequest> context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("RPC Request: {A} {Op} {B}",
            context.Message.A, context.Message.Operation, context.Message.B);

        var result = context.Message.Operation switch
        {
            "add" => context.Message.A + context.Message.B,
            "subtract" => context.Message.A - context.Message.B,
            "multiply" => context.Message.A * context.Message.B,
            "divide" => context.Message.A / context.Message.B,
            _ => throw new ArgumentException("Invalid operation")
        };

        return new CalculateResponse(result, $"Result: {result}");
    }

    public override Task<ErrorHandlingResult> OnErrorAsync(
        RpcContext<CalculateRequest> context,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        _logger.LogError(exception, "RPC error");
        return Task.FromResult(ErrorHandlingResult.Reject);
    }
}
```

### RPC Client (Making Requests)

```csharp
public class CalculationService
{
    private readonly IMessageRpcClient _rpcClient;

    public CalculationService(IMessageRpcClient rpcClient)
    {
        _rpcClient = rpcClient;
    }

    public async Task<int> CalculateAsync(int a, int b, string operation)
    {
        var request = new CalculateRequest(a, b, operation);

        try
        {
            // Make RPC call with 5 second timeout
            var response = await _rpcClient.CallAsync<CalculateRequest, CalculateResponse>(
                exchange: "rpc.exchange",
                routingKey: "calculator",
                request: request,
                timeoutMs: 5000);

            return response.Result;
        }
        catch (TimeoutException)
        {
            // Handle timeout
            throw new InvalidOperationException("RPC call timed out");
        }
    }
}
```

### Minimal API Example

```csharp
app.MapGet("/calculate", async (IMessageRpcClient rpcClient, int a, int b, string op) =>
{
    try
    {
        var request = new CalculateRequest(a, b, op);
        var response = await rpcClient.CallAsync<CalculateRequest, CalculateResponse>(
            "rpc.exchange", "calculator", request, timeoutMs: 5000);
        
        return Results.Ok(response);
    }
    catch (TimeoutException)
    {
        return Results.Problem("RPC timeout", statusCode: 504);
    }
});
```

### RPC Features

- ✅ **Type-Safe** - Generic request/response types
- ✅ **Timeout Support** - Configurable timeouts per call
- ✅ **Correlation Tracking** - Automatic correlation ID management
- ✅ **Exclusive Callback Queue** - One callback queue per RPC client
- ✅ **Error Handling** - Customizable error handling on server side
- ✅ **DI Integration** - Full dependency injection support

### RPC Context Properties

| Property | Type | Description |
|----------|------|-------------|
| `Message` | TRequest | The request message |
| `MessageId` | string | Unique message identifier |
| `CorrelationId` | string? | Request-response correlation ID |
| `ReplyTo` | string? | Callback queue name |
| `Headers` | IDictionary? | Message headers |
| `Timestamp` | DateTime | Message timestamp |
| `Exchange` | string | Source exchange |
| `RoutingKey` | string | Routing key |
| `IsRpcRequest` | bool | Whether this is an RPC request |

### RPC Best Practices

1. **Keep RPC handlers fast** - Use async I/O, avoid blocking operations
2. **Set appropriate timeouts** - Match timeout to expected processing time
3. **Handle errors gracefully** - Return meaningful error responses
4. **Use direct exchanges** - More efficient for point-to-point RPC
5. **Monitor response times** - Add telemetry to RPC handlers
6. **Consider idempotency** - RPC calls may be retried on timeout

---

## API Reference

| Type | Description |
|------|-------------|
| `IMessagePublisher` | Publish messages |
| `IMessageRpcClient` | Make RPC calls |
| `MessageConsumer<T>` | Base class for consumers (requires `[Queue]` attribute) |
| `RpcMessageConsumer<TRequest, TResponse>` | Base class for RPC servers |
| `ConsumeContext<T>` | Message context with metadata |
| `RpcContext<TRequest>` | RPC request context with response capability |
| `PublishOptions` | Publishing options |

| ErrorHandlingResult | Behavior |
|---------------------|----------|
| `Acknowledge` | Remove from queue |
| `Reject` | Reject (goes to DLQ) |
| `Requeue` | Put back for retry |

---

## License

MIT License

## Author

**Ahmed Abuelnour**
