using Flaminco.AuditTrail.Core.Factory;
using Flaminco.AuditTrail.Core.Tracker;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.AuditTrail.Core.Extension;

public static class AuditTrailFactoryExtension
{
    public static IServiceCollection AddAuditTrailMapper<TScanner>(this IServiceCollection services)
    {
        IEnumerable<Type>? types = from type in typeof(TScanner).Assembly.GetTypes()
                                   where typeof(IAuditTrailMapper).IsAssignableFrom(type)
                                   select type;

        foreach (Type? item in types ?? Array.Empty<Type>())
        {
            services.AddTransient(typeof(IAuditTrailMapper), item);
        }

        services.AddSingleton<Func<IEnumerable<IAuditTrailMapper>>>(a => () => a.GetService<IEnumerable<IAuditTrailMapper>>()!);

        services.AddSingleton<IAuditTrailMapperFactory, AuditTrailMapperFactory>();

        return services;
    }
}