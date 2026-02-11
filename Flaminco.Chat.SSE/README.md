# LowCodeHub.Chat.SSE

`LowCodeHub.Chat.SSE` is a plug-and-play package for real-time chat using:

- **Redis Streams** for durable, replayable chat events
- **ASP.NET Core .NET 10 native SSE** (`TypedResults.ServerSentEvents`)

It is intentionally focused on the SSE streaming chat case only.

> Core package does **not** include a concrete chat message model. You provide your own model implementing [`IChatStreamMessage`](./Contracts/IChatStreamMessage.cs).

## Features

- SSE-first chat transport
- Redis Streams persistence (ordered stream IDs)
- Native Last-Event-ID replay support
- Heartbeats for long-lived connections
- Stream trimming for bounded storage
- Payload size guardrails
- Endpoint-agnostic design (you map your own HTTP endpoints in your app/POC)

## Installation

```bash
dotnet add package LowCodeHub.Chat.SSE
```

## Program.cs Setup (Caller-Defined Model)

```csharp
using LowCodeHub.Chat.SSE.Extensions;
using LowCodeHub.Chat.SSE.Contracts;

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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddChatSse<AppChatMessage, AppHistoryReader, AppHeartbeatFactory>(options =>
{
    options.ConnectionString = "localhost:6379";
    options.StreamKeyPrefix = "chat:sse:";
    options.ChatMessageEventType = "chat-message"; // default
    options.MaxStreamLength = 100_000;
    options.ReadBatchSize = 100;
    options.PollIntervalMs = 250;
    options.HeartbeatIntervalSeconds = 15;
    options.MaxPayloadBytes = 16 * 1024;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/chat/{channel}/messages", async (
        string channel,
        SendAppChatRequest request,
        IChatMessagePublisher publisher,
        CancellationToken cancellationToken) =>
    {
        var message = new AppChatMessage(
            MessageId: Guid.NewGuid().ToString("N"),
            Channel: channel,
            SenderId: request.SenderId,
            SenderDisplayName: string.IsNullOrWhiteSpace(request.SenderDisplayName) ? request.SenderId : request.SenderDisplayName,
            Content: request.Content,
            SentAtUtc: DateTimeOffset.UtcNow,
            Metadata: request.Metadata);

        var streamId = await publisher.PublishAsync(message, cancellationToken);
        return Results.Accepted(value: new { streamId, messageId = message.MessageId });
    });

app.MapGet("/chat/{channel}/events", (
        string channel,
        HttpContext httpContext,
        IChatSseStreamService streamService) =>
    {
        httpContext.Request.Headers.TryGetValue("Last-Event-ID", out var lastEventHeader);

        string? lastEventId = lastEventHeader.FirstOrDefault();

        var stream = streamService.StreamAsync<AppChatMessage>(
            channel,
            lastEventId,
            httpContext.RequestAborted);

        return TypedResults.ServerSentEvents(stream);
    });

app.Run();

sealed class AppHistoryReader : IChatHistoryReader<AppChatMessage>
{
    public async IAsyncEnumerable<ChatHistoryEntry<AppChatMessage>> ReadAfterAsync(
        string channel,
        string? lastEventId,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Example only: replace by your repository/EF query.
        await Task.CompletedTask;
        yield break;
    }
}

sealed class AppHeartbeatFactory : IChatHeartbeatFactory<AppChatMessage>
{
    public AppChatMessage Create(string channel)
        => new(
            MessageId: "heartbeat",
            Channel: channel,
            SenderId: "system",
            SenderDisplayName: "system",
            Content: "heartbeat",
            SentAtUtc: DateTimeOffset.UtcNow,
            Metadata: new Dictionary<string, string> { ["type"] = "heartbeat" });
}
```

## Endpoints (mapped by your app)

- `POST /chat/{channel}/messages`
  - Request body is your own DTO.

- `GET /chat/{channel}/events`
  - SSE stream of your caller-defined message model
  - Live event type is configurable via `ChatMessageEventType` (default: `chat-message`)
  - Can replay from caller-provided history reader (`IChatHistoryReader<TMessage>`) before Redis live stream
  - Supports `Last-Event-ID` header for replay

## Browser Client Example

```javascript
const es = new EventSource('/chat/general/events');

es.addEventListener('chat-message', (evt) => {
  const message = JSON.parse(evt.data);
  console.log('chat', evt.lastEventId, message.senderDisplayName, message.content);
});

es.addEventListener('heartbeat', (evt) => {
  console.debug('heartbeat', evt.data);
});
```

## Notes

- Ensure Redis is reachable.
- Use stream key prefix per bounded context (for example `chat:sse:tenant-a:`).
- Keep payloads small for high fan-out.
- Mixed-source replay is supported via `IChatHistoryReader<TMessage>`.
- For non-Redis event IDs (e.g. `db:123`), Redis live stream safely falls back to latest stream cursor.

