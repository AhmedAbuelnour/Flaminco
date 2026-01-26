# Flaminco.RedisChannels

The `Flaminco.RedisChannels` library provides Redis-based channel implementations similar to `System.Threading.Channels.Channel<T>`, designed for distributed systems with multi-pod support. It supports both **Redis Streams** (for persistent, ordered messaging with acknowledgment) and **Redis Pub/Sub** (for fire-and-forget real-time notifications), giving you the flexibility to choose the right messaging pattern for your use case.

## Features

- **Channel-like API**: Familiar API similar to .NET's `System.Threading.Channels.Channel<T>`
- **Redis Streams**: Built on Redis Streams for persistence and reliability
- **Redis Pub/Sub**: Fire-and-forget messaging for real-time notifications
- **Multi-Pod Support**: Consumer groups enable load balancing across multiple instances
- **Message Acknowledgment**: Built-in support for acknowledging processed messages (Streams only)
- **Automatic Consumer Groups**: Automatically creates and manages consumer groups (Streams only)
- **Stream Trimming**: Optional automatic trimming of old stream entries
- **Message Replay**: Native support for replaying missed messages using `ReadFromMessageIdAsync`

## Getting Started

### Installation

To install the `Flaminco.RedisChannels` package, use the following command in the .NET CLI:

```shell
dotnet add package Flaminco.RedisChannels
```

### Setup

Add the `Flaminco.RedisChannels` package to your project and configure it to work with your Redis connection.

```csharp
using Flaminco.RedisChannels.Extensions;

// Configure Redis Streams
services.AddRedisStreams("Your Redis Connection String", options =>
{
    options.DefaultConsumerGroupName = "my-app-group";
    options.ConsumerName = "pod-1";
    options.MaxLength = 10000; // Optional: trim stream to 10k entries
    options.BlockTimeMs = 5000; // Wait 5 seconds for new messages
    options.BatchSize = 10; // Read 10 messages per batch
});
```

### Usage

#### Real-Time Chat with Server-Sent Events (SSE) - .NET 10

This example demonstrates a real-time chat system where both customers and admins can send messages and receive them via Server-Sent Events (SSE).

> **Note**: This example uses .NET 10's native SSE API (`Results.ServerSentEvents` and `SseItem<T>`). For .NET 9.0 or earlier, you can use the manual SSE formatting approach shown in the alternative example below, or upgrade to .NET 10 for the native support.

**1. Chat Message Model:**

```csharp
public record ChatMessage(
    string Id,
    string UserId,
    string UserName,
    string UserRole, // "customer" or "admin"
    string Content,
    DateTime Timestamp
);
```

**2. Chat Controller with Send and SSE Endpoints (using .NET 10 native SSE support):**

> **Note**: This example uses .NET 10's native `Results.ServerSentEvents()` API which automatically handles SSE formatting, headers, and connection management. For more details, see [Milan Jovanović's article on SSE in .NET 10](https://www.milanjovanovic.tech/blog/server-sent-events-in-aspnetcore-and-dotnet-10).

```csharp
using Flaminco.RedisChannels.Abstractions;
using Flaminco.RedisChannels.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IServiceProvider serviceProvider, ILogger<ChatController> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    // POST endpoint to send a chat message
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request, CancellationToken cancellationToken)
    {
        var channel = _serviceProvider.CreateRedisStreamChannel<ChatMessage>(
            "chat-messages",
            consumerGroup: "chat-broadcasters",
            consumerName: $"sender-{Environment.MachineName}");

        var message = new ChatMessage(
            Id: Guid.NewGuid().ToString(),
            UserId: request.UserId,
            UserName: request.UserName,
            UserRole: request.UserRole,
            Content: request.Content,
            Timestamp: DateTime.UtcNow
        );

        var success = await channel.Writer.WriteAsync(message, cancellationToken);
        
        if (success)
        {
            return Ok(new { messageId = message.Id, timestamp = message.Timestamp });
        }

        return StatusCode(500, "Failed to send message");
    }

    // GET endpoint for SSE using .NET 10 native SSE support
    // Both customers and admins can connect - each connection gets all messages
    [HttpGet("stream")]
    public IResult StreamMessages(
        [FromQuery] string? userId = null,
        [FromHeader(Name = "Last-Event-ID")] string? lastEventId = null,
        CancellationToken cancellationToken = default)
    {
        // Each SSE connection gets its own unique consumer group
        // This ensures each client receives ALL messages (broadcast pattern)
        var uniqueGroupId = Guid.NewGuid().ToString("N");
        var channel = _serviceProvider.CreateRedisStreamChannel<ChatMessage>(
            "chat-messages",
            consumerGroup: $"chat-client-{uniqueGroupId}", // Unique group per connection
            consumerName: $"sse-{userId ?? "anonymous"}-{uniqueGroupId}");

        async IAsyncEnumerable<SseItem<ChatMessage>> StreamChatMessages()
        {
            // Replay missed events if client reconnected with Last-Event-ID
            // The library natively supports reading from a specific message ID using await foreach
            if (!string.IsNullOrWhiteSpace(lastEventId))
            {
                _logger.LogInformation("Client reconnected with Last-Event-ID: {LastEventId}, replaying missed messages", lastEventId);
                
                // Use the library's native ReadFromMessageIdAsync to replay missed messages
                // This uses await foreach internally, making it efficient and fully async
                await foreach (var (message, messageId) in channel.Reader.ReadFromMessageIdAsync(lastEventId, cancellationToken))
                {
                    if (message is not null)
                    {
                        yield return new SseItem<ChatMessage>(message, messageId);
                    }
                }
            }

            // Stream new messages as they arrive from Redis Stream
            // Read with IDs so we can include them in SseItem for reconnection support
            while (!cancellationToken.IsCancellationRequested)
            {
                var (message, messageId) = await channel.Reader.ReadWithIdAsync(cancellationToken);
                
                if (message is null)
                {
                    // Wait a bit before checking again
                    await Task.Delay(100, cancellationToken);
                    continue;
                }

                // .NET 10 SseItem automatically handles SSE formatting and event IDs
                // The message Id is used for Last-Event-ID reconnection handling
                yield return new SseItem<ChatMessage>(message, messageId);
            }
        }

        // .NET 10 native SSE support - handles all formatting, headers, and connection management
        return TypedResults.ServerSentEvents(StreamChatMessages(), eventType: "chat-message");
    }
}

public record SendMessageRequest(
    string UserId,
    string UserName,
    string UserRole,
    string Content
);
```

**3. Program.cs Configuration:**

```csharp
using Flaminco.RedisChannels.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Redis Streams for chat
builder.Services.AddRedisStreams(
    builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379",
    options =>
    {
        options.DefaultConsumerGroupName = "chat-app";
        options.ConsumerName = Environment.MachineName;
        options.MaxLength = 10000; // Keep last 10k messages
        options.BlockTimeMs = 1000; // Poll every second for new messages
        options.BatchSize = 10; // Read up to 10 messages per batch
        options.AutoAcknowledge = true; // Auto-acknowledge for SSE (fire-and-forget)
        options.ReadPendingMessagesFirst = false; // For SSE, start from new messages only
    });

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**4. Client-Side JavaScript Example:**

```javascript
// Connect to SSE endpoint - browser automatically handles reconnection
const eventSource = new EventSource('/api/chat/stream');

// Listen for the specific 'chat-message' event type defined in C#
eventSource.addEventListener('chat-message', function(event) {
    const message = JSON.parse(event.data);
    console.log(`[${message.userRole}] ${message.userName}: ${message.content}`);
    console.log(`Event ID: ${event.lastEventId}`); // Used for reconnection
    // Add message to UI
    addMessageToChat(message);
});

// Handle connection opened
eventSource.onopen = function() {
    console.log('SSE connection opened');
};

// Handle generic messages (if any)
eventSource.onmessage = function(event) {
    console.log('Received message:', event);
};

// Handle errors and reconnections
// Browser automatically reconnects and sends Last-Event-ID header
eventSource.onerror = function(error) {
    if (eventSource.readyState === EventSource.CONNECTING) {
        console.log('Reconnecting...');
    } else {
        console.error('SSE connection error:', error);
    }
};

// Send a message
async function sendMessage(userId, userName, userRole, content) {
    await fetch('/api/chat/send', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ userId, userName, userRole, content })
    });
}

// Example usage:
// Customer sends message
sendMessage('customer-1', 'John Doe', 'customer', 'Hello, I need help!');

// Admin sends message
sendMessage('admin-1', 'Support Agent', 'admin', 'How can I help you?');

// Close connection when done
// eventSource.close();
```

**How It Works:**

1. **Send Message (POST `/api/chat/send`)**: 
   - Creates a `ChatMessage` and writes it to the Redis stream using the writer
   - The message is persisted in Redis and available to all consumers

2. **Stream Messages (GET `/api/chat/stream`)**:
   - Uses .NET 10's native `TypedResults.ServerSentEvents()` API
   - Automatically sets `Content-Type: text/event-stream` and other required headers
   - Each client connection creates its own unique consumer group
   - Uses `IAsyncEnumerable<SseItem<ChatMessage>>` to stream messages
   - `SseItem<T>` wraps messages with IDs for automatic reconnection handling
   - Browser's `EventSource` automatically sends `Last-Event-ID` header on reconnection
   - Both customers and admins receive all messages in real-time

3. **Multi-Pod Support**:
   - Multiple API instances can run simultaneously
   - Each SSE connection is independent with its own consumer
   - Redis Streams ensures all clients receive messages regardless of which pod they're connected to
   - If a pod crashes, other pods continue serving their connected clients

4. **Consumer Groups for Broadcasting**:
   - Each SSE connection uses its own unique consumer group
   - This ensures every client receives ALL messages (broadcast pattern)
   - Alternative: Use the same consumer group if you want load balancing (each message goes to one client only)

**Note**: For production, consider:
- Adding authentication/authorization (use `.RequireAuthorization()` on the endpoint)
- Implementing a message buffer to replay missed messages on reconnection (using `Last-Event-ID`)
- Filtering messages by user role if needed (filter in the `IAsyncEnumerable` before yielding)
- Adding rate limiting for message sending
- Using `SseItem<T>` with retry intervals for better connection resilience

**Key .NET 10 SSE Features Used:**
- `TypedResults.ServerSentEvents()` - Native SSE endpoint helper
- `SseItem<T>` - Wraps data with event IDs for reconnection support
- `IAsyncEnumerable<T>` - Streams data efficiently
- Automatic `Last-Event-ID` header handling for reconnection

#### Basic Producer/Consumer Pattern

**Using IAsyncEnumerable (Recommended - Auto-acknowledgment enabled by default):**

```csharp
using Flaminco.RedisChannels.Abstractions;
using Flaminco.RedisChannels.Extensions;

// In your service
public class OrderService
{
    private readonly IServiceProvider _serviceProvider;
    
    public OrderService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task ProcessOrdersAsync(CancellationToken cancellationToken = default)
    {
        // Create a channel for orders
        var channel = _serviceProvider.CreateRedisStreamChannel<Order>("orders-stream");
        
        // Producer: Write orders to the stream
        var writer = channel.Writer;
        await writer.WriteAsync(new Order { Id = 1, Amount = 100.00m });
        
        // Consumer: Read orders from the stream using IAsyncEnumerable
        // Messages are automatically acknowledged after being read (when AutoAcknowledge is true)
        await foreach (var order in channel.Reader.WithCancellation(cancellationToken))
        {
            if (order != null)
            {
                // Process the order
                await ProcessOrderAsync(order);
                // Message is already acknowledged automatically
            }
        }
    }
}
```

**Using ReadAsync with Manual Acknowledgment:**

```csharp
// If you need manual control over acknowledgment, set AutoAcknowledge = false in configuration
var reader = channel.Reader;

while (await reader.WaitToReadAsync())
{
    var (order, messageId) = await reader.ReadWithIdAsync();
    
    if (order != null)
    {
        // Process the order
        await ProcessOrderAsync(order);
        
        // Manually acknowledge the message after successful processing
        await reader.AcknowledgeAsync(messageId);
    }
}
```
```

#### Background Service Consumer

```csharp
using Flaminco.RedisChannels.Abstractions;
using Flaminco.RedisChannels.Extensions;
using Microsoft.Extensions.Hosting;

public class OrderProcessorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    
    public OrderProcessorService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = _serviceProvider.CreateRedisStreamChannel<Order>(
            "orders-stream",
            consumerGroup: "order-processors",
            consumerName: Environment.MachineName);
        
        try
        {
            // Using IAsyncEnumerable - messages are auto-acknowledged
            await foreach (var order in channel.Reader.WithCancellation(stoppingToken))
            {
                if (order != null)
                {
                    try
                    {
                        // Process the order
                        await ProcessOrderAsync(order, stoppingToken);
                        // Message is automatically acknowledged after successful read
                    }
                    catch (Exception ex)
                    {
                        // Log error - message was already acknowledged, so it won't be redelivered
                        // If you need retry logic, set AutoAcknowledge = false and acknowledge manually
                        Console.WriteLine($"Error processing order: {ex.Message}");
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
    }
    
    private async Task ProcessOrderAsync(Order order, CancellationToken cancellationToken)
    {
        // Your order processing logic here
        await Task.Delay(100, cancellationToken);
    }
}
```
```

#### Multiple Consumers (Load Balancing)

When multiple pods/instances use the same consumer group, Redis Streams automatically distributes messages across all consumers:

```csharp
// Pod 1
var channel1 = serviceProvider.CreateRedisStreamChannel<Order>(
    "orders-stream",
    consumerGroup: "order-processors",
    consumerName: "pod-1");

// Pod 2
var channel2 = serviceProvider.CreateRedisStreamChannel<Order>(
    "orders-stream",
    consumerGroup: "order-processors",
    consumerName: "pod-2");

// Both pods will receive different messages from the same stream
// If a pod crashes, unacknowledged messages are redelivered to other pods
```

#### Disabling Auto-Acknowledgment

If you need manual control over acknowledgment (e.g., for retry logic):

```csharp
// Configure with AutoAcknowledge = false
services.AddRedisStreams("connection-string", options =>
{
    options.AutoAcknowledge = false; // Disable automatic acknowledgment
});

// Then manually acknowledge after successful processing
var reader = channel.Reader;

while (await reader.WaitToReadAsync())
{
    var (order, messageId) = await reader.ReadWithIdAsync();
    
    if (order != null)
    {
        try
        {
            await ProcessOrderAsync(order);
            // Acknowledge only after successful processing
            await reader.AcknowledgeAsync(messageId);
        }
        catch
        {
            // Don't acknowledge - message will be redelivered
        }
    }
}
```

#### Redis Pub/Sub Example

**Creating and Using a Pub/Sub Channel:**

```csharp
using Flaminco.RedisChannels.Abstractions;
using Flaminco.RedisChannels.Extensions;

// In your service or controller
public class NotificationService
{
    private readonly IServiceProvider _serviceProvider;
    
    public NotificationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    // Publisher: Send notifications
    public async Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        // Create a pub/sub channel - much simpler than streams (no consumer groups needed)
        var channel = _serviceProvider.CreateRedisPubSubChannel<Notification>("notifications");
        
        // Publish notification - all subscribers will receive it immediately
        var success = await channel.Writer.WriteAsync(notification, cancellationToken);
        
        if (!success)
        {
            throw new InvalidOperationException("Failed to publish notification");
        }
    }
    
    // Subscriber: Listen for notifications
    public async Task ListenForNotificationsAsync(CancellationToken cancellationToken = default)
    {
        // Create a pub/sub channel to receive notifications
        var channel = _serviceProvider.CreateRedisPubSubChannel<Notification>("notifications");
        
        // Use IAsyncEnumerable to receive notifications in real-time
        await foreach (var notification in channel.Reader.WithCancellation(cancellationToken))
        {
            if (notification != null)
            {
                await ProcessNotificationAsync(notification);
                // No acknowledgment needed - pub/sub is fire-and-forget
            }
        }
    }
    
    private async Task ProcessNotificationAsync(Notification notification)
    {
        // Your notification processing logic here
        Console.WriteLine($"Received notification: {notification.Message}");
    }
}

public record Notification(string UserId, string Message, DateTime Timestamp);
```

**Background Service with Pub/Sub:**

```csharp
using Flaminco.RedisChannels.Abstractions;
using Flaminco.RedisChannels.Extensions;
using Microsoft.Extensions.Hosting;

public class NotificationListenerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    
    public NotificationListenerService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Create pub/sub channel - no consumer groups or names needed
        var channel = _serviceProvider.CreateRedisPubSubChannel<Notification>("notifications");
        
        try
        {
            // All instances of this service will receive all notifications (broadcast pattern)
            await foreach (var notification in channel.Reader.WithCancellation(stoppingToken))
            {
                if (notification != null)
                {
                    try
                    {
                        await ProcessNotificationAsync(notification, stoppingToken);
                        // Message is already gone - pub/sub doesn't persist messages
                    }
                    catch (Exception ex)
                    {
                        // Log error - message cannot be recovered
                        Console.WriteLine($"Error processing notification: {ex.Message}");
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
    }
    
    private async Task ProcessNotificationAsync(Notification notification, CancellationToken cancellationToken)
    {
        // Your notification processing logic here
        await Task.Delay(100, cancellationToken);
    }
}
```

**Key Differences from Streams:**

```csharp
// Streams - requires stream key, consumer group, and consumer name
var streamChannel = serviceProvider.CreateRedisStreamChannel<Order>(
    "orders-stream",
    consumerGroup: "order-processors",
    consumerName: "pod-1");

// Pub/Sub - only requires channel name (much simpler!)
var pubSubChannel = serviceProvider.CreateRedisPubSubChannel<Notification>("notifications");

// Both use the same API pattern
await streamChannel.Writer.WriteAsync(order);
await pubSubChannel.Writer.WriteAsync(notification);

await foreach (var item in streamChannel.Reader.WithCancellation(ct)) { }
await foreach (var item in pubSubChannel.Reader.WithCancellation(ct)) { }
```

## How It Works

### Redis Streams

Redis Streams provide:
- **Persistence**: Messages are stored in Redis, surviving restarts
- **Consumer Groups**: Multiple consumers can read from the same stream with load balancing
- **Message Acknowledgment**: Messages can be acknowledged after processing
- **Automatic Redelivery**: Unacknowledged messages are redelivered to other consumers

### Consumer Groups

When you create a channel with a consumer group:
- Each consumer in the group gets different messages (load balancing)
- If a consumer crashes, its unacknowledged messages are redelivered
- Multiple pods can process messages in parallel

### Message Flow

1. **Producer** writes messages to the stream using `Writer.WriteAsync()`
2. **Consumer** reads messages using `Reader.ReadWithIdAsync()`
3. **Consumer** processes the message
4. **Consumer** acknowledges the message using `Reader.AcknowledgeAsync()`
5. If a consumer crashes before acknowledging, the message is redelivered to another consumer

## Configuration Options

- **DefaultConsumerGroupName**: Default consumer group name for all channels
- **ConsumerName**: Default consumer name (auto-generated if not specified)
- **MaxLength**: Maximum stream length before trimming (null = no trimming)
- **BlockTimeMs**: How long to wait for new messages when polling (default: 5000ms)
- **BatchSize**: Number of messages to read per batch (default: 1)
- **ReadPendingMessagesFirst**: Read unacknowledged messages first on startup (default: true)
- **AutoAcknowledge**: Automatically acknowledge messages after reading (default: true)

## Best Practices

### For Redis Streams

1. **Use IAsyncEnumerable**: Prefer `await foreach` pattern for cleaner code and automatic acknowledgment
2. **Auto-Acknowledgment**: Use `AutoAcknowledge = true` (default) for simple cases. Set to `false` only if you need manual control for retry logic
3. **Error Handling**: With auto-acknowledgment, handle errors in your processing logic. Messages are acknowledged immediately after reading
4. **Consumer Names**: Use unique consumer names per instance (e.g., pod name, machine name)
5. **Stream Trimming**: Set `MaxLength` to prevent streams from growing indefinitely
6. **Batch Size**: Adjust `BatchSize` based on your message processing rate
7. **Pending Messages**: Keep `ReadPendingMessagesFirst = true` to recover from crashes
8. **Message Replay**: Use `ReadFromMessageIdAsync` for replaying missed messages after reconnection

### For Redis Pub/Sub

1. **Fire-and-Forget**: Remember that pub/sub messages are not persisted - if a subscriber is down, messages are lost
2. **Real-Time Only**: Use pub/sub for notifications and events that don't need persistence
3. **No Acknowledgment**: Messages are delivered once and cannot be replayed
4. **Broadcast Pattern**: All subscribers receive all messages - use for notifications, not for load balancing
5. **Error Handling**: Handle errors gracefully - missed messages cannot be recovered

## License

This project is licensed under the MIT License.
