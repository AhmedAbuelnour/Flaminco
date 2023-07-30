using Flaminco.AuditTrail.Core.Extension;
using Flaminco.AuditTrail.Core.Tracker;
using Flaminco.AuditTrail.Memory.Tracker;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.AuditTrail.Memory.Extension;

public static class AuditTrailFactoryExtension
{
    public static IServiceCollection AddAuditTrail<TScanner>(this IServiceCollection services)
    {
        return services.AddAuditTrailMapper<TScanner>()
                       .AddAuditTrailTracker<TScanner>()
                       .AddMemoryCache();
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