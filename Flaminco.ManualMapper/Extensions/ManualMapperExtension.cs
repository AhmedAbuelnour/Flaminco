using Flaminco.ManualMapper.Abstractions;
using Flaminco.ManualMapper.Implementations;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Flaminco.ManualMapper.Extensions;

public static class ManualMapperExtension
{
    public static IServiceCollection AddManualMapper<TScanner>(this IServiceCollection services)
    {
        IEnumerable<TypeInfo>? types = from type in typeof(TScanner).Assembly.DefinedTypes
                                       where !type.IsAbstract && type.GetInterfaces().Any(i =>
                                           i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(IMapHandler<,>) ||
                                                               i.GetGenericTypeDefinition() == typeof(IMapAsyncHandler<,>) ||
                                                               i.GetGenericTypeDefinition() == typeof(IMapStreamHandler<,>)))
                                       select type.GetTypeInfo();

        foreach (var (typeInfo, implementedInterface) in (types ?? []).SelectMany(typeInfo => typeInfo.ImplementedInterfaces.Select(implementedInterface => (typeInfo, implementedInterface))))
        {
            services.AddTransient(implementedInterface, typeInfo);
        }

        services.AddTransient<IMapper, DefaultManualMapper>();

        return services;
    }
}