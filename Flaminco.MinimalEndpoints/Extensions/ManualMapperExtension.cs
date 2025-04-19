using Flaminco.MinimalEndpoints.Abstractions;
using Flaminco.MinimalEndpoints.Implementations;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Flaminco.MinimalEndpoints.Extensions;

public static class ManualMapperExtension
{
    /// <summary>
    /// Registers all implementations of <see cref="IDualMapper{TFrom, TTo}"/> found in the assembly of the specified scanner type.
    /// </summary>
    /// <typeparam name="TScanner">The type used to locate the assembly to scan for dual mappers.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddDualMappers<TScanner>(this IServiceCollection services)
    {
        IEnumerable<TypeInfo> types = from type in typeof(TScanner).Assembly.DefinedTypes
                                      where !type.IsAbstract && type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDualMapper<,>))
                                      select type.GetTypeInfo();

        foreach (var (typeInfo, implementedInterface) in (types
            ?? []).SelectMany(typeInfo => typeInfo.ImplementedInterfaces.Select(implementedInterface => (typeInfo, implementedInterface))))
        {
            services.AddTransient(implementedInterface, typeInfo);
        }

        return services;
    }

    public static IServiceCollection AddManualMapper<TScanner>(this IServiceCollection services)
    {
        IEnumerable<TypeInfo>? types = from type in typeof(TScanner).Assembly.DefinedTypes
                                       where !type.IsAbstract && type.GetInterfaces().Any(i =>
                                           i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapHandler<,>))
                                       select type.GetTypeInfo();

        foreach (var (typeInfo, implementedInterface) in (types ?? []).SelectMany(typeInfo => typeInfo.ImplementedInterfaces.Select(implementedInterface => (typeInfo, implementedInterface))))
        {
            services.AddTransient(implementedInterface, typeInfo);
        }

        services.AddTransient<IMapper, DefaultManualMapper>();

        return services;
    }
}