using Flaminco.RedisChannels.Abstractions;
using Flaminco.RedisChannels.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Flaminco.RedisChannels.Implementations;

/// <summary>
///     Provides a writing implementation for a Redis Pub/Sub channel.
/// </summary>
/// <typeparam name="T">The type of data in the channel.</typeparam>
public sealed class RedisPubSubWriter<T> : IRedisPubSubWriter<T>
{
    private readonly ISubscriber _subscriber;
    private readonly RedisChannel _channel;
    private readonly RedisStreamConfiguration _config;
    private bool _completed;
    private Exception? _completionError;

    public RedisPubSubWriter(
        IOptions<RedisStreamConfiguration> options,
        string channelName)
    {
        _config = options.Value;
        _subscriber = _config.ConnectionMultiplexer.GetSubscriber();
        _channel = RedisChannel.Literal(channelName);
    }

    public bool CanWrite => !_completed;

    public async ValueTask<bool> WriteAsync(T item, CancellationToken cancellationToken = default)
    {
        if (_completed)
            return false;

        if (_completionError is not null)
            throw new InvalidOperationException("Channel has been completed with an error.", _completionError);

        try
        {
            var json = RedisChannelSerializer.Serialize(item, _config);
            var subscribers = await _subscriber.PublishAsync(_channel, json);
            return subscribers > 0 || true; // Return true even if no subscribers (message was published)
        }
        catch (RedisConnectionException)
        {
            // Connection issue - caller should retry
            return false;
        }
        catch (RedisServerException)
        {
            // Server error - caller should retry
            return false;
        }
        catch (Exception)
        {
            // Other errors
            return false;
        }
    }

    public ValueTask<bool> WaitToWriteAsync(CancellationToken cancellationToken = default)
    {
        if (_completed)
            return ValueTask.FromResult(false);

        if (_completionError is not null)
            return ValueTask.FromResult(false);

        return ValueTask.FromResult(true);
    }

    public void Complete(Exception? error = null)
    {
        _completed = true;
        _completionError = error;
    }
}
