# Flaminco.RabbitMQ.AMQP

A powerful, modern .NET library that provides a clean abstraction layer over RabbitMQ.Client 7.2 for ASP.NET Core applications targeting .NET 10.

## Features

- **Modern Async API** - Built entirely on async/await patterns
- **Automatic Connection Recovery** - Built-in resilience with automatic reconnection
- **Publisher Confirms** - Reliable message delivery with broker confirmations
- **Dead Letter Queue Support** - Automatic DLQ creation and routing
- **Hierarchical Topology Config** - Exchanges contain their queues with bindings and DLQ - all in one place
- **Pluggable Serialization** - Use JSON (default) or implement custom serializers
- **Health Checks** - Built-in health check for connection monitoring

## Installation

```shell
dotnet add package Flaminco.RabbitMQ.AMQP
```

---

## Quick Start

```csharp
// Program.cs
using Flaminco.RabbitMQ.AMQP.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRabbitMq(builder.Configuration);

var app = builder.Build();
app.Run();
```

---

## Configuration (appsettings.json)

### Compact Hierarchical Structure

Exchanges contain their queues, bindings, and dead-letter config all in one place:

```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "AutomaticRecoveryEnabled": true,
    "DefaultPrefetchCount": 10,

    "Topology": {
      "Exchanges": [
        {
          "Name": "orders",
          "Type": "topic",
          "Queues": [
            {
              "Name": "orders.created",
              "RoutingKey": "order.created.#",
              "PrefetchCount": 20,
              "DeadLetter": {
                "Queue": "orders.created.dlq"
              }
            },
            {
              "Name": "orders.shipped",
              "RoutingKey": "order.shipped.#",
              "DeadLetter": {}
            }
          ]
        },
        {
          "Name": "notifications",
          "Type": "fanout",
          "Queues": [
            {
              "Name": "notifications.email",
              "RoutingKey": ""
            },
            {
              "Name": "notifications.sms",
              "RoutingKey": ""
            }
          ]
        }
      ],

      "Queues": [
        {
          "Name": "background.jobs",
          "MaxLength": 10000,
          "DeadLetter": {
            "Queue": "background.jobs.dlq"
          }
        }
      ]
    }
  }
}
```

### What Gets Created Automatically

From the above config:

| Created | Details |
|---------|---------|
| Exchange `orders` | Topic exchange |
| Exchange `orders.dlx` | Auto-created dead-letter exchange |
| Queue `orders.created` | Bound to `orders` with key `order.created.#` |
| Queue `orders.created.dlq` | Auto-created, bound to `orders.dlx` |
| Queue `orders.shipped` | Bound to `orders` with key `order.shipped.#` |
| Queue `orders.shipped.dlq` | Auto-created (defaults to `{queueName}.dlq`) |
| Exchange `notifications` | Fanout exchange |
| Queue `notifications.email` | Bound to `notifications` |
| Queue `notifications.sms` | Bound to `notifications` |
| Queue `background.jobs` | Standalone queue (default exchange) |
| Queue `background.jobs.dlq` | Auto-created DLQ |

### DeadLetter Configuration

When you specify `DeadLetter: {}`, the system automatically creates:
- DLX exchange: `{parentExchange}.dlx` (or custom name)
- DLQ queue: `{queueName}.dlq` (or custom name)
- Binding between them

```json
{
  "Name": "my.queue",
  "RoutingKey": "my.#",
  "DeadLetter": {
    "Exchange": "custom.dlx",      // Optional - defaults to {exchange}.dlx
    "Queue": "custom.dlq",         // Optional - defaults to {queue}.dlq
    "RoutingKey": "custom.key",    // Optional - defaults to {queue}.dlq
    "MessageTtl": 60000            // Optional - TTL for DLQ messages
  }
}
```

### Queue Options

```json
{
  "Name": "orders.priority",
  "RoutingKey": "order.priority.#",
  "Durable": true,
  "Exclusive": false,
  "AutoDelete": false,
  "MessageTtl": 86400000,
  "MaxLength": 10000,
  "MaxLengthBytes": 104857600,
  "MaxPriority": 10,
  "QueueMode": "lazy",
  "PrefetchCount": 50,
  "DeadLetter": {}
}
```

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

    // Publish to exchange
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

## Consuming Messages

Extend `MessageConsumer<T>` and add `[Queue]` attribute:

```csharp
using Flaminco.RabbitMQ.AMQP.Abstractions;
using Flaminco.RabbitMQ.AMQP.Attributes;

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

### Consumer with All Lifecycle Hooks

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

### Using Attributes for Topology

If you prefer to define topology on consumers instead of JSON:

```csharp
[Exchange("orders", Type = "topic")]
[Queue("orders.processor", DeadLetterExchange = "orders.dlx")]
[Binding("orders", RoutingKey = "order.#")]
public class OrderProcessor : MessageConsumer<OrderMessage>
{
    public override Task ConsumeAsync(ConsumeContext<OrderMessage> context, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
```

---

## Fluent Topology API

```csharp
builder.Services.AddRabbitMq(
    options =>
    {
        options.HostName = "localhost";
        options.UserName = "guest";
        options.Password = "guest";
    },
    topology =>
    {
        topology
            .DeclareExchange("orders", ExchangeTypes.Topic)
            .DeclareQueueWithDeadLetter("orders.created", "orders.dlx")
            .BindQueue("orders.created", "orders", "order.created.#");
    });
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

builder.Services.AddRabbitMq(configuration).UseMessageSerializer<MessagePackSerializer>();
```

---

## SSL/TLS

```json
{
  "RabbitMQ": {
    "HostName": "rabbitmq.example.com",
    "Ssl": {
      "Enabled": true,
      "ServerName": "rabbitmq.example.com",
      "CertificatePath": "/certs/client.pfx",
      "CertificatePassword": "secret"
    }
  }
}
```

---

## Clustering

```json
{
  "RabbitMQ": {
    "Endpoints": [
      { "HostName": "rabbit1.example.com", "Port": 5672 },
      { "HostName": "rabbit2.example.com", "Port": 5672 }
    ]
  }
}
```

---

## API Reference

| Type | Description |
|------|-------------|
| `IMessagePublisher` | Publish messages |
| `MessageConsumer<T>` | Base class for consumers |
| `ConsumeContext<T>` | Message context with metadata |
| `PublishOptions` | Publishing options |

| Attribute | Description |
|-----------|-------------|
| `[Queue]` | Declare queue for consumer |
| `[Exchange]` | Declare exchange |
| `[Binding]` | Bind queue to exchange |

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
