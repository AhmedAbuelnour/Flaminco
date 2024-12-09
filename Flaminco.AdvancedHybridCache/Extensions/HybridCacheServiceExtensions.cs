using Flaminco.AdvancedHybridCache.Abstractions;
using Flaminco.AdvancedHybridCache.Implementations;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Flaminco.AdvancedHybridCache.Extensions
{
    public static class HybridCacheServiceExtensions
    {
        public static IServiceCollection AddAdvancedHybridCache(this IServiceCollection services, Action<HybridCacheOptions>? hybridCacheOptions = default)
        {
            if (hybridCacheOptions is null)
            {
                services.AddHybridCache();
            }
            else
            {
                services.AddHybridCache(hybridCacheOptions);
            }

            services.TryAddSingleton<IAdvancedHybridCache, AdvancedHybridCacheImp>();

            return services;
        }
    }
}
