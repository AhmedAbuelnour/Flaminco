using Flaminco.RedisChannels.Abstractions;
using Flaminco.RedisChannels.Exceptions;
using Flaminco.RedisChannels.Implementations;
using Flaminco.RedisChannels.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;
using StackExchange.Redis;

namespace Flaminco.RedisChannels.Extensions;

/// <summary>
///     Contains extension methods for configuring Redis Streams in the dependency injection container.
/// </summary>
public static class RedisStreamExtensions
{
    /// <summary>
    ///     Adds Redis Streams support to the dependency injection container.
    /// </summary>
    /// <param name="services">The collection of service descriptors.</param>
    /// <param name="redisConnection">The connection string for the Redis server.</param>
    /// <param name="configure">Optional action to configure Redis Stream options.</param>
    /// <returns>The modified service collection.</returns>
    /// <exception cref="EmptyRedisConnectionException">Thrown when the Redis connection string is empty or null.</exception>
    public static IServiceCollection AddRedisStreams(
        this IServiceCollection services,
        string redisConnection,
        Action<RedisStreamConfiguration>? configure = null)
    {
        if (string.IsNullOrEmpty(redisConnection))
            throw new EmptyRedisConnectionException(redisConnection);

        services.Configure<RedisStreamConfiguration>(opt =>
        {
            opt.ConnectionMultiplexer = ConnectionMultiplexer.Connect(redisConnection);
            configure?.Invoke(opt);
        });

        return services;
    }

    /// <summary>
    ///     Adds Redis Streams support and registers a typed Redis-backed event bus.
    ///     This is useful when replacing in-memory Channel-based event buses for distributed deployments.
    /// </summary>
    /// <typeparam name="TEventBase">The base event contract type (for example, IDomainEvent).</typeparam>
    /// <param name="services">The collection of service descriptors.</param>
    /// <param name="redisConnection">The connection string for the Redis server.</param>
    /// <param name="streamKey">The Redis stream key used as the event bus transport.</param>
    /// <param name="knownTypeAssemblies">Optional assemblies that contain concrete event types.</param>
    /// <param name="configure">Optional action to configure Redis Stream options.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddRedisEventBus<TEventBase>(
        this IServiceCollection services,
        string redisConnection,
        string streamKey,
        IEnumerable<Assembly>? knownTypeAssemblies = null,
        Action<RedisStreamConfiguration>? configure = null)
    {
        services.AddRedisStreams(redisConnection, options =>
        {
            options.EnablePolymorphicSerialization = true;
            options.AddKnownTypeAssemblies(typeof(TEventBase).Assembly);

            if (knownTypeAssemblies is not null)
            {
                foreach (Assembly assembly in knownTypeAssemblies)
                    options.AddKnownTypeAssemblies(assembly);
            }

            configure?.Invoke(options);
        });

        services.AddSingleton<IRedisEventBus<TEventBase>>(provider =>
        {
            IOptions<RedisStreamConfiguration> options = provider.GetRequiredService<IOptions<RedisStreamConfiguration>>();
            return new RedisEventBus<TEventBase>(options, streamKey);
        });

        return services;
    }

    /// <summary>
    ///     Creates a Redis Stream channel for the specified stream key.
    /// </summary>
    /// <typeparam name="T">The type of data in the channel.</typeparam>
    /// <param name="services">The service provider.</param>
    /// <param name="streamKey">The Redis stream key.</param>
    /// <param name="consumerGroup">Optional consumer group name. If not specified, uses the default from configuration.</param>
    /// <param name="consumerName">Optional consumer name. If not specified, uses the default from configuration.</param>
    /// <returns>A Redis Stream channel instance.</returns>
    public static IRedisStreamChannel<T> CreateRedisStreamChannel<T>(
        this IServiceProvider services,
        string streamKey,
        string? consumerGroup = null,
        string? consumerName = null)
    {
        var options = services.GetRequiredService<IOptions<RedisStreamConfiguration>>();

        return new RedisStreamChannel<T>(options, streamKey, consumerGroup, consumerName);
    }

    /// <summary>
    ///     Creates a Redis Pub/Sub channel for the specified channel name.
    ///     Pub/Sub is fire-and-forget messaging - messages are not persisted and cannot be replayed.
    /// </summary>
    /// <typeparam name="T">The type of data in the channel.</typeparam>
    /// <param name="services">The service provider.</param>
    /// <param name="channelName">The Redis pub/sub channel name.</param>
    /// <returns>A Redis Pub/Sub channel instance.</returns>
    public static IRedisPubSubChannel<T> CreateRedisPubSubChannel<T>(
        this IServiceProvider services,
        string channelName)
    {
        var options = services.GetRequiredService<IOptions<RedisStreamConfiguration>>();
        return new RedisPubSubChannel<T>(options, channelName);
    }

    /// <summary>
    ///     Creates a Redis-backed event bus that mirrors the in-memory Channel event bus pattern.
    /// </summary>
    /// <typeparam name="TEventBase">The base event contract type.</typeparam>
    /// <param name="services">The service provider.</param>
    /// <param name="streamKey">The Redis stream key.</param>
    /// <param name="consumerGroup">Optional consumer group name.</param>
    /// <param name="consumerName">Optional consumer name.</param>
    /// <returns>A Redis-backed event bus instance.</returns>
    public static IRedisEventBus<TEventBase> CreateRedisEventBus<TEventBase>(
        this IServiceProvider services,
        string streamKey,
        string? consumerGroup = null,
        string? consumerName = null)
    {
        var options = services.GetRequiredService<IOptions<RedisStreamConfiguration>>();
        return new RedisEventBus<TEventBase>(options, streamKey, consumerGroup, consumerName);
    }
}
