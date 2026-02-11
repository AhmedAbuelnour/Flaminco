using Flaminco.Chat.SSE.Contracts;
using Flaminco.Chat.SSE.Extensions;
using Flaminco.MinimalEndpoints.Abstractions;
using Flaminco.MinimalEndpoints.Extensions;
using Microsoft.Data.Sqlite;
using StackExchange.Redis;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


//builder.Services.AddRabbitMq(builder.Configuration, topology =>
//{
//    // Topic exchange for order events
//    topology.Exchange("orders.exchange", exchange => exchange.Topic()
//                                                    .WithDeadLetterExchange()  // Creates "orders.dlx" automatically
//                                                    .Queue("poc.order.created.queue", "order.created.#", queue => queue.DeadLetter()));  // Creates "orders.created.dlq" automatically

//    // Direct exchange for RPC
//    topology.Exchange("rpc.exchange", exchange => exchange.Direct()
//                                                    .Queue("calculator.queue", "calculator"));

//    //    *(star)Matches exactly one word order.*.new matches order.created.new but not order.created.new.customer
//    //    # (hash)	Matches zero or more words	order.created.# matches order.created, order.created.new, order.created.new.customer, etc.
//    //    . (dot) Word separator  Separates words in the routing key
//});

// Add RPC client support
//builder.Services.AddRabbitMqRpc();


builder.Services.AddValidators<Program>();

builder.Services.AddRedisEventBus(options =>
{
    // Important: BLPOP can block up to PopTimeoutSeconds, so Redis client timeout must be higher.
    // Otherwise you'll frequently see RedisTimeoutException when the queue is empty.
    var redisConfiguration = new ConfigurationOptions
    {
        EndPoints = { "localhost:6379" },
        AbortOnConnectFail = false,
        ConnectTimeout = 15000,
        SyncTimeout = 15000,
        AsyncTimeout = 15000
    };

    options.ConnectionString = "localhost:6379";
    options.ConnectionMultiplexer = ConnectionMultiplexer.Connect(redisConfiguration); // Reuse the connection multiplexer for better performance
    options.QueueKey = "poc.redis.events"; // Redis list key for the event queue
    options.PopTimeoutSeconds = 3; // Keep below Redis client timeout to avoid false timeout exceptions on idle queues
}, typeof(Program).Assembly);



builder.Services.AddFlamincoChatSse<AppChatMessage, AppChatHistoryReader, AppChatHeartbeatFactory>(options =>
{
    options.ConnectionString = "localhost:6379";
    options.StreamKeyPrefix = "poc.chat.sse:";
    options.ChatMessageEventType = "chat-message";
    options.MaxStreamLength = 10_000;
    options.UseApproximateTrimming = true;
    options.PollIntervalMs = 200;
    options.ReplayBatchSize = 100;
    options.HeartbeatIntervalSeconds = 10;
    options.MaxPayloadBytes = 8 * 1024;
});

var historyDbPath = Path.Combine(AppContext.BaseDirectory, "poc-chat-history.db");
builder.Services.AddSingleton(new ChatHistoryDb($"Data Source={historyDbPath}"));

var app = builder.Build();

await app.Services.GetRequiredService<ChatHistoryDb>().InitializeAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/chat/{channel}/messages", async (
        string channel,
        SendAppChatRequest request,
        IChatMessagePublisher publisher,
        ChatHistoryDb historyDb,
        CancellationToken cancellationToken) =>
    {
        if (string.IsNullOrWhiteSpace(channel))
            return Results.BadRequest("Channel is required.");

        if (request is null)
            return Results.BadRequest("Request body is required.");

        var message = new AppChatMessage(
            MessageId: Guid.NewGuid().ToString("N"),
            Channel: channel,
            SenderId: request.SenderId,
            SenderDisplayName: string.IsNullOrWhiteSpace(request.SenderDisplayName) ? request.SenderId : request.SenderDisplayName,
            Content: request.Content,
            SentAtUtc: DateTimeOffset.UtcNow,
            Metadata: request.Metadata);

        var streamId = await publisher.PublishAsync(message, cancellationToken);
        await historyDb.SaveMessageAsync(message, cancellationToken);

        return Results.Accepted(value: new
        {
            streamId,
            messageId = message.MessageId,
            channel = message.Channel,
            sentAtUtc = message.SentAtUtc
        });
    })
    .WithName("FlamincoChatSse.Publish");

app.MapGet("/chat/{channel}/events", (
        string channel,
        HttpContext httpContext,
        IChatSseStreamService streamService) =>
    {
        if (string.IsNullOrWhiteSpace(channel))
            return Results.BadRequest("Channel is required.");

        httpContext.Request.Headers.TryGetValue("Last-Event-ID", out var lastEventHeader);
        string? lastEventId = lastEventHeader.FirstOrDefault();

        IAsyncEnumerable<SseItem<AppChatMessage>> stream = streamService.StreamAsync<AppChatMessage>(
            channel,
            lastEventId,
            httpContext.RequestAborted);

        return TypedResults.ServerSentEvents(stream);
    })
    .WithName("FlamincoChatSse.Stream");


app.MapGet("/Get", async (IEventBus eventBus) =>
{
    await eventBus.Publish(new OrderCreated
    {
        OrderId = 1,
        CreatedAt = DateTime.UtcNow
    }, CancellationToken.None);

});


//// Publisher endpoint for regular messages
//app.MapGet("/publisher", async (IMessagePublisher publisher, IAttachmentGetAllOperation attachmentOperation) =>
//{
//    await publisher.PublishAsync("orders.exchange", "order.created", new PublisMessageTest
//    {
//        Id = 1
//    });

//    return Results.Ok("Message published successfully");
//});


//// RPC client endpoint
//app.MapGet("/calculate", async (IMessageRpcClient rpcClient, int a = 10, int b = 5, string operation = "add") =>
//{
//    try
//    {
//        var request = new CalculateRequest
//        {
//            A = a,
//            B = b,
//            Operation = operation
//        };

//        var response = await rpcClient.CallAsync<CalculateRequest, CalculateResponse>(
//            "rpc.exchange",
//            "calculator",
//            request,
//            timeoutMs: 5000);

//        return Results.Ok(new
//        {
//            request = request,
//            response = response
//        });
//    }
//    catch (TimeoutException ex)
//    {
//        return Results.Problem($"RPC call timed out: {ex.Message}", statusCode: 504);
//    }
//    catch (Exception ex)
//    {
//        return Results.Problem($"RPC call failed: {ex.Message}", statusCode: 500);
//    }
//});

app.Run();


public class OrderCreated : IDomainEvent
{
    public int OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderCreatedHandler : IDomainEventHandler<OrderCreated>
{
    private readonly ILogger<OrderCreatedHandler> _logger;
    public OrderCreatedHandler(ILogger<OrderCreatedHandler> logger)
    {
        _logger = logger;
    }

    public ValueTask Handle(OrderCreated domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling OrderCreated event for OrderId: {OrderId} at {CreatedAt}", domainEvent.OrderId, domainEvent.CreatedAt);
        return ValueTask.CompletedTask;
    }
}

public sealed record AppChatMessage(
    string MessageId,
    string Channel,
    string SenderId,
    string SenderDisplayName,
    string Content,
    DateTimeOffset SentAtUtc,
    IReadOnlyDictionary<string, string>? Metadata = null) : IChatStreamMessage;

public sealed class SendAppChatRequest
{
    public string SenderId { get; set; } = string.Empty;
    public string SenderDisplayName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; set; }
}


internal sealed class AppChatHistoryReader : IChatHistoryReader<AppChatMessage>
{
    private readonly ChatHistoryDb _db;

    public AppChatHistoryReader(ChatHistoryDb db)
    {
        _db = db;
    }

    public IAsyncEnumerable<ChatHistoryEntry<AppChatMessage>> ReadAfterAsync(
        string channel,
        string? lastEventId,
        CancellationToken cancellationToken = default)
    {
        return _db.LoadHistoryAfterAsync(channel, lastEventId, cancellationToken);
    }
}

internal sealed class AppChatHeartbeatFactory : IChatHeartbeatFactory<AppChatMessage>
{
    public AppChatMessage Create(string channel)
    {
        return new AppChatMessage(
            MessageId: "heartbeat",
            Channel: channel,
            SenderId: "system",
            SenderDisplayName: "system",
            Content: "heartbeat",
            SentAtUtc: DateTimeOffset.UtcNow,
            Metadata: new Dictionary<string, string> { ["type"] = "heartbeat" });
    }
}



// DTO classes
class PublisMessageTest
{
    public int Id { get; set; }
}

internal sealed class ChatHistoryDb
{
    private readonly string _connectionString;

    public ChatHistoryDb(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS chat_messages (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                channel TEXT NOT NULL,
                message_id TEXT NOT NULL,
                sender_id TEXT NOT NULL,
                sender_display_name TEXT NOT NULL,
                content TEXT NOT NULL,
                sent_at_utc TEXT NOT NULL,
                metadata_json TEXT NULL
            );

            CREATE INDEX IF NOT EXISTS ix_chat_messages_channel_sent_at
                ON chat_messages(channel, sent_at_utc, id);
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SaveMessageAsync(AppChatMessage message, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO chat_messages (
                channel,
                message_id,
                sender_id,
                sender_display_name,
                content,
                sent_at_utc,
                metadata_json)
            VALUES (
                $channel,
                $messageId,
                $senderId,
                $senderDisplayName,
                $content,
                $sentAtUtc,
                $metadataJson);
            """;

        command.Parameters.AddWithValue("$channel", message.Channel);
        command.Parameters.AddWithValue("$messageId", message.MessageId);
        command.Parameters.AddWithValue("$senderId", message.SenderId);
        command.Parameters.AddWithValue("$senderDisplayName", message.SenderDisplayName);
        command.Parameters.AddWithValue("$content", message.Content);
        command.Parameters.AddWithValue("$sentAtUtc", message.SentAtUtc.UtcDateTime.ToString("O"));
        command.Parameters.AddWithValue("$metadataJson", message.Metadata is null ? DBNull.Value : JsonSerializer.Serialize(message.Metadata));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async IAsyncEnumerable<ChatHistoryEntry<AppChatMessage>> LoadHistoryAfterAsync(
        string channel,
        string? lastEventId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var lastSeenAtUtc = ParseEventIdToTimestamp(lastEventId);
        if (!lastSeenAtUtc.HasValue)
            yield break;

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT message_id, sender_id, sender_display_name, content, sent_at_utc, metadata_json
            FROM chat_messages
            WHERE channel = $channel
              AND sent_at_utc > $lastSeenAtUtc
            ORDER BY sent_at_utc, id
            LIMIT 500;
            """;

        command.Parameters.AddWithValue("$channel", channel);
        command.Parameters.AddWithValue("$lastSeenAtUtc", lastSeenAtUtc.Value.UtcDateTime.ToString("O"));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var sentAtUtcRaw = reader.GetString(4);
            if (!DateTimeOffset.TryParse(sentAtUtcRaw, out var sentAtUtc))
                continue;

            var metadataJson = reader.IsDBNull(5) ? null : reader.GetString(5);

            var message = new AppChatMessage(
                MessageId: reader.GetString(0),
                Channel: channel,
                SenderId: reader.GetString(1),
                SenderDisplayName: reader.GetString(2),
                Content: reader.GetString(3),
                SentAtUtc: sentAtUtc,
                Metadata: DeserializeMetadata(metadataJson));

            var eventId = GenerateEventId(sentAtUtc);
            yield return new ChatHistoryEntry<AppChatMessage>(
                EventId: eventId,
                Message: message,
                EventType: "chat-message");
        }
    }

    private static IReadOnlyDictionary<string, string>? DeserializeMetadata(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(metadataJson);
        }
        catch
        {
            return null;
        }
    }

    private static DateTimeOffset? ParseEventIdToTimestamp(string? eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
            return null;

        var value = eventId.Trim();

        if (value == "0")
            return null;

        if (value.StartsWith("db:", StringComparison.OrdinalIgnoreCase))
            value = value[3..];

        var dashIndex = value.IndexOf('-');
        if (dashIndex > 0)
            value = value[..dashIndex];

        if (!long.TryParse(value, out var unixMs))
            return null;

        try
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(unixMs);
        }
        catch
        {
            return null;
        }
    }

    private static string GenerateEventId(DateTimeOffset timestamp)
    {
        return timestamp.ToUnixTimeMilliseconds().ToString();
    }
}

class ConsumerMessageTest
{
    public int Id { get; set; }
}

class CalculateRequest
{
    public int A { get; set; }
    public int B { get; set; }
    public string Operation { get; set; } = string.Empty;
}

class CalculateResponse
{
    public int Result { get; set; }
    public string Message { get; set; } = string.Empty;
}

//// Regular message consumer
//[Queue("poc.order.created.queue")]
//class PublishMessageConsumer : MessageConsumer<ConsumerMessageTest>
//{
//    private readonly ILogger<PublishMessageConsumer> _logger;

//    public PublishMessageConsumer(ILogger<PublishMessageConsumer> logger)
//    {
//        _logger = logger;
//    }

//    public override async Task ConsumeAsync(ConsumeContext<ConsumerMessageTest> context, CancellationToken cancellationToken = default)
//    {
//        _logger.LogInformation("Received order created message with Id: {Id}", context.Message.Id);
//        await Task.CompletedTask;
//    }
//}

//// RPC server consumer
//[Queue("calculator.queue")]
//class CalculatorRpcConsumer : RpcMessageConsumer<CalculateRequest, CalculateResponse>
//{
//    private readonly ILogger<CalculatorRpcConsumer> _logger;

//    public CalculatorRpcConsumer(ILogger<CalculatorRpcConsumer> logger)
//    {
//        _logger = logger;
//    }

//    public override async Task<CalculateResponse> HandleAsync(
//        RpcContext<CalculateRequest> context,
//        CancellationToken cancellationToken = default)
//    {
//        _logger.LogInformation("Received RPC request: {A} {Operation} {B}",
//            context.Message.A, context.Message.Operation, context.Message.B);

//        var result = context.Message.Operation.ToLower() switch
//        {
//            "add" => context.Message.A + context.Message.B,
//            "subtract" => context.Message.A - context.Message.B,
//            "multiply" => context.Message.A * context.Message.B,
//            "divide" => context.Message.B != 0 ? context.Message.A / context.Message.B : throw new DivideByZeroException(),
//            _ => throw new ArgumentException($"Invalid operation: {context.Message.Operation}")
//        };

//        _logger.LogInformation("Calculated result: {Result}", result);

//        await Task.Delay(100, cancellationToken); // Simulate some processing

//        return new CalculateResponse
//        {
//            Result = result,
//            Message = $"{context.Message.A} {context.Message.Operation} {context.Message.B} = {result}"
//        };
//    }

//    public override async Task<ErrorHandlingResult> OnErrorAsync(
//        RpcContext<CalculateRequest> context,
//        Exception exception,
//        CancellationToken cancellationToken = default)
//    {
//        _logger.LogError(exception, "Error processing RPC request");

//        // For RPC, we typically want to reject failed requests
//        // so the caller gets a timeout instead of waiting indefinitely
//        return await Task.FromResult(ErrorHandlingResult.Reject);
//    }
//}
