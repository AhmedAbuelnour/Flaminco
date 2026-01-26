using Flaminco.RedisChannels.Abstractions;
using Flaminco.RedisChannels.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Flaminco.RedisChannels.Implementations;

/// <summary>
///     Provides a reading implementation for a Redis Stream.
/// </summary>
/// <typeparam name="T">The type of data in the stream.</typeparam>
public sealed class RedisStreamReader<T> : IRedisStreamReader<T>, IDisposable
{
    private readonly IDatabase _database;
    private readonly string _streamKey;
    private readonly string _consumerGroup;
    private readonly string _consumerName;
    private readonly RedisStreamConfiguration _config;
    private readonly ConcurrentQueue<(T Item, string MessageId)> _buffer = new();
    private bool _completionRequested;
    private Task? _readingTask;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private bool _hasReadPendingMessages;

    public RedisStreamReader(IOptions<RedisStreamConfiguration> options,
                             string streamKey,
                             string? consumerGroup = null,
                             string? consumerName = null)
    {
        _config = options.Value;
        _database = _config.ConnectionMultiplexer.GetDatabase();
        _streamKey = streamKey;
        _consumerGroup = consumerGroup ?? _config.DefaultConsumerGroupName ?? $"{streamKey}-group";
        _consumerName = consumerName ?? _config.ConsumerName ?? $"{Environment.MachineName}-{Guid.NewGuid():N}";

        // Ensure consumer group exists
        EnsureConsumerGroupAsync().GetAwaiter().GetResult();

        // Start background reading task
        _readingTask = Task.Run(ReadFromStreamAsync, _cancellationTokenSource.Token);
    }

    public bool CanRead => !_completionRequested && !_cancellationTokenSource.Token.IsCancellationRequested;

    public bool Completion => _completionRequested;

    public async ValueTask<T?> ReadAsync(CancellationToken cancellationToken = default)
    {
        (T? item, string? messageId) = await ReadWithIdAsync(cancellationToken);

        // Auto-acknowledge if enabled and message was successfully read
        if (_config.AutoAcknowledge && item is not null && !string.IsNullOrEmpty(messageId))
        {
            await AcknowledgeAsync(messageId, cancellationToken);
        }

        return item;
    }

    public async ValueTask<(T? Item, string MessageId)> ReadWithIdAsync(CancellationToken cancellationToken = default)
    {
        if (_completionRequested && _buffer.IsEmpty)
            return (default, string.Empty);

        // Try to read from buffer first
        if (_buffer.TryDequeue(out var entry))
        {
            return (entry.Item, entry.MessageId);
        }

        // Wait for data to be available
        if (await WaitToReadAsync(cancellationToken))
        {
            if (_buffer.TryDequeue(out entry))
            {
                return (entry.Item, entry.MessageId);
            }
        }

        return (default, string.Empty);
    }

    public async ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default)
    {
        if (_completionRequested)
            return false;

        if (!_buffer.IsEmpty)
            return true;

        // Wait a bit for the background task to fill the buffer
        var timeout = TimeSpan.FromMilliseconds(_config.BlockTimeMs);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);
        cts.CancelAfter(timeout);

        try
        {
            await Task.Delay(100, cts.Token);
            return !_buffer.IsEmpty;
        }
        catch (OperationCanceledException)
        {
            return !_buffer.IsEmpty;
        }
    }

    public async ValueTask AcknowledgeAsync(string messageId, CancellationToken cancellationToken = default)
    {
        if (_database is null)
            return;

        await _database.StreamAcknowledgeAsync(_streamKey, _consumerGroup, messageId);
    }

    private async Task EnsureConsumerGroupAsync()
    {
        try
        {
            await _database.StreamCreateConsumerGroupAsync(_streamKey, _consumerGroup, "0", createStream: true);
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
        {
            // Consumer group already exists, which is fine
        }
        catch (RedisConnectionException)
        {
            // Connection issue - will be retried in ReadFromStreamAsync
            throw;
        }
    }

    private async Task ReadFromStreamAsync()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested && !_completionRequested)
        {
            try
            {
                StreamEntry[] entries;

                // Read pending messages first if configured and not yet done
                if (_config.ReadPendingMessagesFirst && !_hasReadPendingMessages)
                {
                    entries = await _database.StreamReadGroupAsync(
                        _streamKey,
                        _consumerGroup,
                        _consumerName,
                        position: "0",
                        count: _config.BatchSize);

                    if (entries is { Length: 0 })
                    {
                        // No pending messages, switch to new messages
                        _hasReadPendingMessages = true;
                        entries = await _database.StreamReadGroupAsync(
                            _streamKey,
                            _consumerGroup,
                            _consumerName,
                            position: ">",
                            count: _config.BatchSize);
                    }
                    else
                    {
                        _hasReadPendingMessages = true;
                    }
                }
                else
                {
                    // Read new messages
                    entries = await _database.StreamReadGroupAsync(
                        _streamKey,
                        _consumerGroup,
                        _consumerName,
                        position: ">",
                        count: _config.BatchSize);
                }

                if (entries is { Length: > 0 })
                {
                    foreach (var entry in entries)
                    {
                        if (entry.Values.Length > 0)
                        {
                            // Find the "data" field
                            var dataField = entry.Values.FirstOrDefault(v => v.Name == "data");
                            if (dataField.Name == "data" && !dataField.Value.IsNullOrEmpty)
                            {
                                var jsonValue = dataField.Value.ToString();
                                if (!string.IsNullOrEmpty(jsonValue))
                                {
                                    try
                                    {
                                        var item = JsonSerializer.Deserialize<T>(jsonValue);
                                        if (item is not null)
                                        {
                                            _buffer.Enqueue((item, entry.Id.ToString()));
                                        }
                                    }
                                    catch (JsonException)
                                    {
                                        // Skip invalid JSON entries - acknowledge to prevent reprocessing
                                        await _database.StreamAcknowledgeAsync(_streamKey, _consumerGroup, entry.Id);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    // No messages available - wait before next poll (implements blocking behavior)
                    await Task.Delay(_config.BlockTimeMs, _cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (RedisConnectionException)
            {
                // Connection issue - wait longer before retrying
                await Task.Delay(5000, _cancellationTokenSource.Token);
            }
            catch (RedisServerException ex) when (ex.Message.Contains("NOGROUP"))
            {
                // Consumer group doesn't exist - recreate it
                await EnsureConsumerGroupAsync();
                await Task.Delay(1000, _cancellationTokenSource.Token);
            }
            catch (Exception)
            {
                // Other errors - wait a bit before retrying
                await Task.Delay(1000, _cancellationTokenSource.Token);
            }
        }
    }

    public void Dispose()
    {
        if (_completionRequested)
            return;

        _completionRequested = true;
        _cancellationTokenSource.Cancel();

        try
        {
            _readingTask?.Wait(TimeSpan.FromSeconds(5));
        }
        catch (AggregateException)
        {
            // Expected when task is cancelled
        }

        _cancellationTokenSource.Dispose();
    }

    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        while (!_completionRequested && !cancellationToken.IsCancellationRequested)
        {
            T? item = await ReadAsync(cancellationToken);

            if (item is null)
            {
                // No more items available, wait a bit before checking again
                if (await WaitToReadAsync(cancellationToken))
                {
                    continue;
                }
                else
                {
                    // Stream is completed
                    break;
                }
            }

            yield return item;
        }
    }

    public async IAsyncEnumerable<(T Item, string MessageId)> ReadFromMessageIdAsync(string? fromMessageId = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Determine starting position - Redis StreamReadAsync reads messages AFTER the specified ID
        // So we use the message ID directly (not ">" which is for consumer groups)
        string lastReadId = string.IsNullOrWhiteSpace(fromMessageId) ? "0" : fromMessageId;

        // Read messages from the specified position
        // Using StreamReadAsync (not StreamReadGroupAsync) to read all messages for replay
        // This allows replaying messages that were already consumed
        while (!_cancellationTokenSource.Token.IsCancellationRequested && !_completionRequested && !cancellationToken.IsCancellationRequested)
        {
            StreamEntry[] entries;

            try
            {
                // Read messages from the stream starting after the last message ID
                entries = await _database.StreamReadAsync(_streamKey,
                                                          lastReadId,
                                                          count: _config.BatchSize);
            }
            catch (OperationCanceledException)
            {
                yield break;
            }
            catch (Exception)
            {
                // On error, wait a bit before retrying
                await Task.Delay(1000, cancellationToken);
                continue;
            }

            if (entries is { Length: > 0 })
            {
                // Process entries outside try-catch to allow yield
                foreach (var entry in entries)
                {
                    if (entry.Values.Length > 0)
                    {
                        // Find the "data" field
                        var dataField = entry.Values.FirstOrDefault(v => v.Name == "data");
                        if (dataField.Name == "data" && !dataField.Value.IsNullOrEmpty)
                        {
                            var jsonValue = dataField.Value.ToString();
                            if (!string.IsNullOrEmpty(jsonValue))
                            {
                                T? item = default;
                                try
                                {
                                    item = JsonSerializer.Deserialize<T>(jsonValue);
                                }
                                catch (JsonException)
                                {
                                    // Skip invalid JSON entries
                                    continue;
                                }

                                if (item is not null)
                                {
                                    var messageId = entry.Id.ToString();
                                    lastReadId = messageId;
                                    yield return (item, messageId);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // No more messages to replay, break out of the loop
                yield break;
            }
        }
    }
}
