using Flaminco.RediPolly.Abstractions;
using Flaminco.RediPolly.Exceptions;
using Flaminco.RediPolly.Implementations;
using Flaminco.RediPolly.Options;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Flaminco.RediPolly.Extensions;

/// <summary>
///     Contains extension methods for configuring Redis channels in the dependency injection container.
/// </summary>
public static class RedisChannelExtensions
{
    /// <summary>
    ///     Adds Redis channels to the dependency injection container.
    /// </summary>
    /// <typeparam name="TPublisherScanner">The type of the publisher scanner.</typeparam>
    /// <param name="services">The collection of service descriptors.</param>
    /// <param name="redisConnection">The connection string for the Redis server.</param>
    /// <returns>The modified service collection.</returns>
    /// <exception cref="EmptyRedisConnectionException">Thrown when the Redis connection string is empty or null.</exception>
    public static IServiceCollection AddRediPolly<TPublisherScanner>(this IServiceCollection services,
        string redisConnection)
    {
        if (string.IsNullOrEmpty(redisConnection))
            throw new EmptyRedisConnectionException(redisConnection);


        services.Configure<RedisChannelConfiguration>(opt =>
        {
            opt.ConnectionMultiplexer = ConnectionMultiplexer.Connect(redisConnection);
        });

        AddPublishers<TPublisherScanner>(services);

        services.AddSingleton<IPublisherLocator, PublisherLocator>();

        return services;
    }

    /// <summary>
    ///     Adds publishers to the dependency injection container based on the specified publisher scanner.
    /// </summary>
    /// <typeparam name="TPublisherScanner">The type of the publisher scanner.</typeparam>
    /// <param name="services">The collection of service descriptors.</param>
    /// <returns>The modified service collection.</returns>
    private static void AddPublishers<TPublisherScanner>(IServiceCollection services)
    {
        var publisherTypes = typeof(TPublisherScanner)
            .Assembly
            .DefinedTypes
            .Where(type => !type.IsAbstract && typeof(ChannelPublisher).IsAssignableFrom(type));

        foreach (var type in publisherTypes) services.AddSingleton(typeof(ChannelPublisher), type);
    }
}