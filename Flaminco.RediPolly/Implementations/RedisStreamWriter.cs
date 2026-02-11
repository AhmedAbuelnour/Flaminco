using Flaminco.RedisChannels.Abstractions;
using Flaminco.RedisChannels.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace Flaminco.RedisChannels.Implementations;

/// <summary>
///     Provides a writing implementation for a Redis Stream.
/// </summary>
/// <typeparam name="T">The type of data in the stream.</typeparam>
public sealed class RedisStreamWriter<T> : IRedisStreamWriter<T>
{
    private readonly IDatabase _database;
    private readonly string _streamKey;
    private readonly RedisStreamConfiguration _config;
    private bool _completed;
    private Exception? _completionError;

    public RedisStreamWriter(
        IOptions<RedisStreamConfiguration> options,
        string streamKey)
    {
        _config = options.Value;
        _database = _config.ConnectionMultiplexer.GetDatabase();
        _streamKey = streamKey;
    }

    public bool CanWrite => !_completed;

    public async ValueTask<bool> WriteAsync(T item, CancellationToken cancellationToken = default)
    {
        if (_completed)
            return false;

        if (_completionError is not null)
            throw new InvalidOperationException("Stream has been completed with an error.", _completionError);

        try
        {
            var json = RedisChannelSerializer.Serialize(item, _config);
            var nameValuePairs = new NameValueEntry[]
            {
                new("data", json)
            };

            var messageId = await _database.StreamAddAsync(
                _streamKey,
                nameValuePairs,
                maxLength: _config.MaxLength,
                useApproximateMaxLength: true);

            return !messageId.IsNull;
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
