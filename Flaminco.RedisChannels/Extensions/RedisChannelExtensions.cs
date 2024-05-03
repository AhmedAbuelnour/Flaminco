
namespace Flaminco.RedisChannels.Extensions
{
    using Flaminco.RedisChannels.Abstractions;
    using Flaminco.RedisChannels.Implementations;
    using Flaminco.RedisChannels.Options;
    using Microsoft.Extensions.DependencyInjection;
    using StackExchange.Redis;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class RedisChannelExtensions
    {
        public static IServiceCollection AddRedisChannels<TPublisherScanner>(this IServiceCollection services, string redisConnection)
        {
            if (!string.IsNullOrEmpty(redisConnection))
            {
                throw new ArgumentNullException(redisConnection, "Redis connection can't be null");
            }

            services.Configure<RedisChannelConfiguration>(opt =>
            {
                opt.ConnectionMultiplexer = ConnectionMultiplexer.Connect(redisConnection);
            });

            services.AddPublishers<TPublisherScanner>();

            services.AddSingleton<IPublisherLocator, PublisherLocator>();

            return services;
        }

        private static IServiceCollection AddPublishers<TPublisherScanner>(this IServiceCollection services)
        {
            IEnumerable<TypeInfo> publisherTypes = from type in typeof(TPublisherScanner).Assembly.DefinedTypes
                                                   where !type.IsAbstract && typeof(ChannelPublisher).IsAssignableFrom(type)
                                                   select type.GetTypeInfo();

            foreach (Type? type in publisherTypes)
            {
                services.AddSingleton(typeof(ChannelPublisher), type);
            }

            return services;
        }
    }
}
