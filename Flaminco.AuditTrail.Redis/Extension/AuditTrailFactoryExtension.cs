using Flaminco.AuditTrail.Core.Extension;
using Flaminco.AuditTrail.Core.Tracker;
using Flaminco.AuditTrail.Redis.Tracker;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.AuditTrail.Redis.Extension;

public static class AuditTrailFactoryExtension
{
    public static IServiceCollection AddAuditTrail<TScanner>(this IServiceCollection services, Action<RedisCacheOptions> setupAction)
    {
        return services.AddAuditTrailMapper<TScanner>()
                       .AddAuditTrailTracker<TScanner>()
                       .AddStackExchangeRedisCache(setupAction);
    }
    static IServiceCollection AddAuditTrailTracker<TScanner>(this IServiceCollection services)
    {
        IEnumerable<Type>? types = from type in typeof(TScanner).Assembly.GetTypes()
                                   where typeof(IAuditTrailTracker<,>).IsAssignableFrom(type)
                                   select type;

        services.AddTransient(typeof(IAuditTrailTracker<,>), typeof(AuditTrailTracker<,>));

        foreach (Type? item in types ?? Array.Empty<Type>())
        {
            if (typeof(AuditTrailTracker<,>) != item)
            {
                services.AddTransient(typeof(IAuditTrailTracker<,>), item);
            }
        }

        return services;
    }
}