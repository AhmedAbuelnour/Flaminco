using Flaminco.RedisChannels.Abstractions;
using Flaminco.RedisChannels.Options;
using Microsoft.Extensions.Options;

namespace Flaminco.RedisChannels.Implementations;

/// <summary>
///     Provides a Redis Pub/Sub-based channel implementation, similar to System.Threading.Channels.Channel&lt;T&gt;.
///     Pub/Sub is fire-and-forget messaging - messages are not persisted and cannot be replayed.
/// </summary>
/// <typeparam name="T">The type of data in the channel.</typeparam>
public sealed class RedisPubSubChannel<T> : IRedisPubSubChannel<T>
{
    public RedisPubSubChannel(
        IOptions<RedisStreamConfiguration> options,
        string channelName)
    {
        Writer = new RedisPubSubWriter<T>(options, channelName);
        Reader = new RedisPubSubReader<T>(options, channelName);
    }

    public IRedisPubSubReader<T> Reader { get; }

    public IRedisPubSubWriter<T> Writer { get; }
}
