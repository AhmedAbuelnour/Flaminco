using Flaminco.RedisChannels.Abstractions;
using Flaminco.RedisChannels.Exceptions;
using Flaminco.RedisChannels.Implementations;
using Flaminco.RedisChannels.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
}
