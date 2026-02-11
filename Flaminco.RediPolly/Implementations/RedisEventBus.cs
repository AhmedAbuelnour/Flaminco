using Flaminco.RedisChannels.Abstractions;
using Flaminco.RedisChannels.Options;
using Microsoft.Extensions.Options;

namespace Flaminco.RedisChannels.Implementations;

/// <summary>
///     Redis-backed event bus implementation that mirrors the common in-memory Channel&lt;T&gt; event bus shape.
/// </summary>
/// <typeparam name="TEventBase">The base event contract type.</typeparam>
public sealed class RedisEventBus<TEventBase> : IRedisEventBus<TEventBase>
{
    public RedisEventBus(
        IOptions<RedisStreamConfiguration> options,
        string streamKey,
        string? consumerGroup = null,
        string? consumerName = null)
    {
        Writer = new RedisStreamWriter<TEventBase>(options, streamKey);
        Reader = new RedisStreamReader<TEventBase>(options, streamKey, consumerGroup, consumerName);
    }

    public IRedisStreamReader<TEventBase> Reader { get; }

    public IRedisStreamWriter<TEventBase> Writer { get; }

    public ValueTask<bool> Publish<TEvent>(TEvent eventItem, CancellationToken cancellationToken = default)
        where TEvent : TEventBase
    {
        return Writer.WriteAsync(eventItem, cancellationToken);
    }
}
