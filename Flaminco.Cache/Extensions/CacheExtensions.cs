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

                services.AddScoped<ICacheService, CacheService>();

                if (cacheConfiguration.Mechanism.Equals("Memory", StringComparison.CurrentCultureIgnoreCase))
                {
                    services.AddDistributedMemoryCache();

                }
                else if (cacheConfiguration.Mechanism.Equals("Redis", StringComparison.CurrentCultureIgnoreCase))
                {
                    ArgumentException.ThrowIfNullOrEmpty(cacheConfiguration.RedisConnectionString);

                    services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = cacheConfiguration.RedisConnectionString;
                        options.InstanceName = cacheConfiguration.InstanceName;
                    });
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
