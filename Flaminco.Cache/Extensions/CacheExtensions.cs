using Flaminco.Cache.Implementations;
using Flaminco.Cache.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.Cache.Extensions
{
    public static class CacheExtensions
    {
        public static IServiceCollection AddCache(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddOptions<CacheConfiguration>().Bind(Configuration.GetSection(nameof(CacheConfiguration)));

            if (Configuration.GetSection(nameof(CacheConfiguration)).Get<CacheConfiguration>() is CacheConfiguration cacheConfiguration)
            {
                ArgumentException.ThrowIfNullOrEmpty(cacheConfiguration.Mechanism);

                if (cacheConfiguration.Mechanism.Equals("Memory", StringComparison.CurrentCultureIgnoreCase))
                {
                    services.AddMemoryCache();

                    services.AddScoped<ICacheService, MemoryCacheService>();
                }
                else if (cacheConfiguration.Mechanism.Equals("Redis", StringComparison.CurrentCultureIgnoreCase))
                {
                    ArgumentException.ThrowIfNullOrEmpty(cacheConfiguration.RedisConnectionString);

                    services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = cacheConfiguration.RedisConnectionString;
                        options.InstanceName = cacheConfiguration.InstanceName;
                    });

                    services.AddScoped<ICacheService, DistributedCacheService>();
                }
                else
                {
                    throw new ArgumentOutOfRangeException(cacheConfiguration!.Mechanism, "The current CacheService only supports (Memory and Redis)");
                }
            }
            else
            {
                throw new ArgumentNullException(nameof(CacheConfiguration), "Ensure to include the cache configuration");
            }

            return services;
        }
    }
}
