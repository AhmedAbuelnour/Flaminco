using LowCodeHub.Chat.SSE.Contracts;
using LowCodeHub.Chat.SSE.Internal;
using LowCodeHub.Chat.SSE.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Net.ServerSentEvents;
using System.Text;
using System.Text.Json;

namespace LowCodeHub.Chat.SSE.Services;

internal sealed class RedisChatSseService : IChatMessagePublisher, IChatSseStreamService, IAsyncDisposable
{
    private const string PayloadField = "payload";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RedisChatSseOptions _options;
    private readonly ILogger<RedisChatSseService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnectionMultiplexer _connection;
    private readonly IDatabase _db;
    private readonly bool _ownsConnection;

    public RedisChatSseService(
        IOptions<RedisChatSseOptions> options,
        ILogger<RedisChatSseService> logger,
        IServiceProvider serviceProvider)
    {
        _options = options.Value;
        _logger = logger;
        _serviceProvider = serviceProvider;

        ValidateOptions(_options);

        if (_options.ConnectionMultiplexer is not null)
        {
            _connection = _options.ConnectionMultiplexer;
            _ownsConnection = false;
        }
        else
        {
            var redisConfig = ConfigurationOptions.Parse(_options.ConnectionString);
            redisConfig.AbortOnConnectFail = false;
            redisConfig.ConnectTimeout = Math.Max(redisConfig.ConnectTimeout, 15000);
            redisConfig.SyncTimeout = Math.Max(redisConfig.SyncTimeout, 15000);
            redisConfig.AsyncTimeout = Math.Max(redisConfig.AsyncTimeout, 15000);
            _connection = ConnectionMultiplexer.Connect(redisConfig);
            _ownsConnection = true;
        }

        _db = _connection.GetDatabase();
    }

    public async ValueTask<string> PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class, IChatStreamMessage
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(message.Channel))
            throw new ArgumentException("Channel is required.", nameof(message));

        if (string.IsNullOrWhiteSpace(message.Content))
            throw new ArgumentException("Message content is required.", nameof(message));

        string payload = JsonSerializer.Serialize(message, SerializerOptions);

        int bytes = Encoding.UTF8.GetByteCount(payload);

        if (bytes > _options.MaxPayloadBytes)
            throw new InvalidOperationException($"Payload size {bytes} exceeds configured max {_options.MaxPayloadBytes} bytes.");

        string streamKey = RedisStreamKeyBuilder.Build(_options, message.Channel);

        NameValueEntry[] values = new NameValueEntry[]
        {
            new(PayloadField, payload)
        };

        RedisValue streamId = await _db.StreamAddAsync(
            streamKey,
            values,
            maxLength: checked((int)_options.MaxStreamLength),
            useApproximateMaxLength: _options.UseApproximateTrimming).ConfigureAwait(false);

        _logger.LogDebug("Published chat message to stream {StreamKey} with id {StreamId}", streamKey, streamId);
        return streamId!;
    }

    public async IAsyncEnumerable<SseItem<TMessage>> StreamAsync<TMessage>(
        string channel,
        string? lastEventId,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TMessage : class, IChatStreamMessage
    {
        if (string.IsNullOrWhiteSpace(channel))
            throw new ArgumentException("Channel is required.", nameof(channel));

        IChatHistoryReader<TMessage>? historyReader = _serviceProvider.GetService<IChatHistoryReader<TMessage>>();

        if (historyReader is not null)
        {
            await foreach (var history in historyReader.ReadAfterAsync(channel, lastEventId, cancellationToken)
                .WithCancellation(cancellationToken)
                .ConfigureAwait(false))
            {
                if (history is null || history.Message is null || string.IsNullOrWhiteSpace(history.EventId))
                    continue;

                var historyEventType = string.IsNullOrWhiteSpace(history.EventType)
                    ? _options.ChatMessageEventType
                    : history.EventType;

                yield return new SseItem<TMessage>(history.Message, historyEventType)
                {
                    EventId = history.EventId
                };
            }
        }

        var streamKey = RedisStreamKeyBuilder.Build(_options, channel);
        var currentId = await ResolveStartIdAsync(streamKey, TryExtractRedisStreamId(lastEventId)).ConfigureAwait(false);
        var heartbeatInterval = TimeSpan.FromSeconds(_options.HeartbeatIntervalSeconds);
        var nextHeartbeat = DateTimeOffset.UtcNow.Add(heartbeatInterval);
        var heartbeatFactory = _serviceProvider.GetService<IChatHeartbeatFactory<TMessage>>();

        while (!cancellationToken.IsCancellationRequested)
        {
            StreamEntry[] entries;

            try
            {
                entries = await _db.StreamReadAsync(streamKey, currentId, count: _options.ReadBatchSize).ConfigureAwait(false);
            }
            catch (RedisException ex)
            {
                _logger.LogWarning(ex, "Redis read failed for channel {Channel}. Retrying.", channel);
                await Task.Delay(_options.PollIntervalMs, cancellationToken).ConfigureAwait(false);
                continue;
            }

            if (entries.Length == 0)
            {
                if (DateTimeOffset.UtcNow >= nextHeartbeat)
                {
                    if (heartbeatFactory is not null)
                    {
                        var heartbeatMessage = heartbeatFactory.Create(channel);
                        yield return new SseItem<TMessage>(heartbeatMessage, "heartbeat")
                        {
                            EventId = currentId
                        };
                    }

                    nextHeartbeat = DateTimeOffset.UtcNow.Add(heartbeatInterval);
                }

                await Task.Delay(_options.PollIntervalMs, cancellationToken).ConfigureAwait(false);
                continue;
            }

            foreach (var entry in entries)
            {
                var payload = entry.Values.FirstOrDefault(x => x.Name == PayloadField).Value;
                if (payload.IsNullOrEmpty)
                    continue;

                currentId = entry.Id;
                TMessage? chatMessage;
                try
                {
                    chatMessage = JsonSerializer.Deserialize<TMessage>(payload.ToString(), SerializerOptions);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize chat payload for stream entry {StreamId}", entry.Id);
                    continue;
                }

                if (chatMessage is null)
                    continue;

                var item = new SseItem<TMessage>(chatMessage, _options.ChatMessageEventType)
                {
                    EventId = entry.Id
                };

                yield return item;
            }
        }
    }

    public ValueTask DisposeAsync()
    {
        if (_ownsConnection)
            _connection.Dispose();

        return ValueTask.CompletedTask;
    }

    private static void ValidateOptions(RedisChatSseOptions options)
    {
        if (options.ConnectionMultiplexer is null && string.IsNullOrWhiteSpace(options.ConnectionString))
            throw new InvalidOperationException("Redis connection is required. Set ConnectionString or ConnectionMultiplexer.");

        if (options.ReadBatchSize <= 0)
            throw new InvalidOperationException("ReadBatchSize must be greater than zero.");

        if (options.MaxPayloadBytes <= 0)
            throw new InvalidOperationException("MaxPayloadBytes must be greater than zero.");

        if (options.PollIntervalMs < 20)
            throw new InvalidOperationException("PollIntervalMs must be >= 20ms.");

        if (options.HeartbeatIntervalSeconds < 5)
            throw new InvalidOperationException("HeartbeatIntervalSeconds must be >= 5 seconds.");

        if (string.IsNullOrWhiteSpace(options.ChatMessageEventType))
            throw new InvalidOperationException("ChatMessageEventType is required.");
    }

    private async Task<string> ResolveStartIdAsync(string streamKey, string? lastEventId)
    {
        // StackExchange.Redis StreamRead does not accept "$" (StreamPosition.NewMessages).
        // We need an explicit id and then read entries strictly greater than that id.
        if (!string.IsNullOrWhiteSpace(lastEventId))
            return lastEventId;

        StreamEntry[] latest = await _db.StreamRangeAsync(
            key: streamKey,
            minId: "-",
            maxId: "+",
            count: 1,
            messageOrder: Order.Descending).ConfigureAwait(false);

        // If stream is empty/non-existing, start from 0-0 (first future message will be > 0-0).
        return latest.Length == 0 ? "0-0" : latest[0].Id.ToString();
    }

    private static string? TryExtractRedisStreamId(string? eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
            return null;

        // Supports mixed-source IDs like "db:123" and redis IDs like "1700000000000-0".
        var value = eventId.Trim();
        var dashIndex = value.IndexOf('-');
        if (dashIndex <= 0 || dashIndex == value.Length - 1)
            return null;

        return long.TryParse(value.AsSpan(0, dashIndex), out _) &&
               long.TryParse(value.AsSpan(dashIndex + 1), out _)
            ? value
            : null;
    }
}

