using Flaminco.RedisChannels.Abstractions;
using Flaminco.RedisChannels.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Flaminco.RedisChannels.Implementations;

/// <summary>
///     Provides a reading implementation for a Redis Pub/Sub channel.
/// </summary>
/// <typeparam name="T">The type of data in the channel.</typeparam>
public sealed class RedisPubSubReader<T> : IRedisPubSubReader<T>, IDisposable
{
    private readonly ISubscriber _subscriber;
    private readonly RedisChannel _channel;
    private readonly ConcurrentQueue<T> _buffer = new();
    private bool _completionRequested;
    private Task? _subscriptionTask;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly SemaphoreSlim _subscriptionReady = new(0, 1);

    public RedisPubSubReader(
        IOptions<RedisStreamConfiguration> options,
        string channelName)
    {
        _subscriber = options.Value.ConnectionMultiplexer.GetSubscriber();
        _channel = RedisChannel.Literal(channelName);

        // Start subscription task
        _subscriptionTask = Task.Run(async () =>
        {
            await SubscribeAsync();
        }, _cancellationTokenSource.Token);
    }

    public bool CanRead => !_completionRequested && !_cancellationTokenSource.Token.IsCancellationRequested;

    public bool Completion => _completionRequested;

    public async ValueTask<T?> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (_completionRequested && _buffer.IsEmpty)
            return default;

        // Try to read from buffer first
        if (_buffer.TryDequeue(out var item))
        {
            return item;
        }

        // Wait for data to be available
        if (await WaitToReadAsync(cancellationToken))
        {
            if (_buffer.TryDequeue(out item))
            {
                return item;
            }
        }

        return default;
    }

    public async ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default)
    {
        if (_completionRequested)
            return false;

        if (!_buffer.IsEmpty)
            return true;

        // Wait a bit for the subscription task to fill the buffer
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);
        cts.CancelAfter(TimeSpan.FromSeconds(5));

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

    private async Task SubscribeAsync()
    {
        try
        {
            // SubscribeAsync returns immediately after registering the subscription
            // The callback will be invoked asynchronously when messages arrive
            await _subscriber.SubscribeAsync(_channel, (channel, value) =>
            {
                if (!_completionRequested && !_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        var json = value.ToString();
                        if (!string.IsNullOrEmpty(json))
                        {
                            // Try to deserialize the message
                            T? item = default;
                            
                            try
                            {
                                item = JsonSerializer.Deserialize<T>(json);
                            }
                            catch (JsonException)
                            {
                                // If generic deserialization fails and T is string, try string-specific deserialization
                                if (typeof(T) == typeof(string))
                                {
                                    try
                                    {
                                        var stringValue = JsonSerializer.Deserialize<string>(json);
                                        if (stringValue is not null)
                                        {
                                            item = (T)(object)stringValue;
                                        }
                                    }
                                    catch
                                    {
                                        // If JSON deserialization fails, try using raw value (might already be a string)
                                        item = (T)(object)json;
                                    }
                                }
                            }
                            
                            if (item is not null)
                            {
                                _buffer.Enqueue(item);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Skip invalid messages
                    }
                }
            });
            
            // Signal that subscription is ready
            _subscriptionReady.Release();
            
            // Keep the subscription alive - this task should run indefinitely
            // The subscription stays active even after SubscribeAsync completes
            await Task.Delay(Timeout.Infinite, _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        catch (Exception)
        {
            // Log error if needed
        }
    }
    
    private async Task EnsureSubscriptionReadyAsync(CancellationToken cancellationToken = default)
    {
        // Wait for subscription to be established (with timeout)
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);
        cts.CancelAfter(TimeSpan.FromSeconds(2));
        
        try
        {
            await _subscriptionReady.WaitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Timeout or cancellation - subscription might still work
        }
    }

    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        // Ensure subscription is ready before starting to read
        await EnsureSubscriptionReadyAsync(cancellationToken);
        
        while (!_completionRequested && !cancellationToken.IsCancellationRequested)
        {
            // Try to read from buffer first
            if (_buffer.TryDequeue(out var item))
            {
                yield return item;
                continue;
            }

            // Wait for messages to arrive (polling the buffer)
            // Pub/Sub messages arrive asynchronously via the subscription callback
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);
            
            // Poll the buffer periodically until we get a message or cancellation
            while (_buffer.IsEmpty && !_completionRequested && !cts.Token.IsCancellationRequested)
            {
                await Task.Delay(50, cts.Token);
                
                if (_buffer.TryDequeue(out item))
                {
                    yield return item;
                    break;
                }
            }
            
            // If completion was requested and buffer is empty, exit
            if (_completionRequested && _buffer.IsEmpty)
            {
                break;
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
            _subscriber.UnsubscribeAsync(_channel).GetAwaiter().GetResult();
            _subscriptionTask?.Wait(TimeSpan.FromSeconds(5));
        }
        catch (AggregateException)
        {
            // Expected when task is cancelled
        }

        _cancellationTokenSource.Dispose();
        _subscriptionReady.Dispose();
    }
}
