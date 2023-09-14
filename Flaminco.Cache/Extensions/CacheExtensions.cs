using Flaminco.Cache.Implementations;
using Flaminco.Cache.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.Cache.Extensions
{
    public static class CacheExtensions
    {
        public static IServiceCollection AddDistributedCache(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddScoped<ICacheService, CacheService>();

            services.AddOptions<CacheConfiguration>().Bind(Configuration.GetSection(nameof(CacheConfiguration)));

            var cacheConfiguration = Configuration.GetSection(nameof(CacheConfiguration)).Get<CacheConfiguration>();

            ArgumentNullException.ThrowIfNull(cacheConfiguration);

            ArgumentException.ThrowIfNullOrEmpty(cacheConfiguration.Mechanism);

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
                throw new ArgumentOutOfRangeException(cacheConfiguration!.Mechanism);
            }

            return services;
        }


        public static IServiceCollection AddMemoryCache(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddScoped<ICacheService, CacheService>();

            services.AddOptions<CacheConfiguration>().Bind(Configuration.GetSection(nameof(CacheConfiguration)));

            var cacheConfiguration = Configuration.GetSection(nameof(CacheConfiguration)).Get<CacheConfiguration>();

            ArgumentNullException.ThrowIfNull(cacheConfiguration);

            services.AddMemoryCache();

            return services;
        }

    }
}
