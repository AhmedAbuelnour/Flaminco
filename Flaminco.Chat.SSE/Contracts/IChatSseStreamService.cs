using System.Net.ServerSentEvents;

namespace LowCodeHub.Chat.SSE.Contracts;

/// <summary>
/// Streams chat messages from Redis Streams as native ASP.NET Core SSE items.
/// </summary>
public interface IChatSseStreamService
{
    /// <summary>
    /// Streams replay + live events for a channel.
    /// </summary>
    IAsyncEnumerable<SseItem<TMessage>> StreamAsync<TMessage>(
        string channel,
        string? lastEventId,
        CancellationToken cancellationToken = default)
        where TMessage : class, IChatStreamMessage;
}

