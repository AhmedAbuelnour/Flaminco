using Flaminco.MinimalEndpoints.Abstractions;
using Flaminco.MinimalEndpoints.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Flaminco.MinimalEndpoints.Extensions;

public static class ManualMapperExtension
{
    public static IServiceCollection AddDualMappers<TScanner>(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var types = typeof(TScanner).Assembly.DefinedTypes
            .Where(t => !t.IsAbstract && t.ImplementedInterfaces.Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDualMapper<,>)));

        foreach (var typeInfo in types)
        {
            var interfaces = typeInfo.ImplementedInterfaces
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDualMapper<,>));

            foreach (var @interface in interfaces)
            {
                services.TryAddEnumerable(ServiceDescriptor.Transient(@interface, typeInfo));
            }
        }

        return services;
    }

    public static IServiceCollection AddManualMapper<TScanner>(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var types = typeof(TScanner).Assembly.DefinedTypes
            .Where(t => !t.IsAbstract && t.ImplementedInterfaces.Any(i =>
                i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(IMapHandler<,>)
                                    || i.GetGenericTypeDefinition() == typeof(IMapAsyncHandler<,>))));

        foreach (var typeInfo in types)
        {
            var interfaces = typeInfo.ImplementedInterfaces
                .Where(i => i.IsGenericType && (
                    i.GetGenericTypeDefinition() == typeof(IMapHandler<,>) ||
                    i.GetGenericTypeDefinition() == typeof(IMapAsyncHandler<,>)));

            foreach (var @interface in interfaces)
            {
                services.TryAddEnumerable(ServiceDescriptor.Transient(@interface, typeInfo));
            }
        }

        services.TryAddTransient<IMapper, DefaultManualMapper>();

        return services;
    }
}
