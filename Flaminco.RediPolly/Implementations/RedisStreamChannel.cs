using Flaminco.RedisChannels.Abstractions;
using Flaminco.RedisChannels.Options;
using Microsoft.Extensions.Options;

namespace Flaminco.RedisChannels.Implementations;

/// <summary>
///     Provides a Redis Stream-based channel implementation, similar to System.Threading.Channels.Channel&lt;T&gt;.
/// </summary>
/// <typeparam name="T">The type of data in the channel.</typeparam>
public sealed class RedisStreamChannel<T> : IRedisStreamChannel<T>
{
    public RedisStreamChannel(
        IOptions<RedisStreamConfiguration> options,
        string streamKey,
        string? consumerGroup = null,
        string? consumerName = null)
    {
        Writer = new RedisStreamWriter<T>(options, streamKey);
        Reader = new RedisStreamReader<T>(options, streamKey, consumerGroup, consumerName);
    }

    public IRedisStreamReader<T> Reader { get; }

    public IRedisStreamWriter<T> Writer { get; }
}
