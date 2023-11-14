using Flaminco.ManualMapper.Abstractions;
using Flaminco.ManualMapper.Implementations;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Flaminco.ManualMapper.Extensions
{
    public static class ManualMapperExtension
    {
        public static IServiceCollection AddManualMapper<T>(this IServiceCollection services)
        {
            IEnumerable<TypeInfo>? types = from type in typeof(T).Assembly.DefinedTypes
                                           where !type.IsAbstract && type.GetInterfaces().Any(i => i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(IMapHandler<,>) || i.GetGenericTypeDefinition() == typeof(IMapAsyncHandler<,>) || i.GetGenericTypeDefinition() == typeof(IMapStreamHandler<,>)))
                                           select type.GetTypeInfo();

            foreach (TypeInfo? typeInfo in types ?? Array.Empty<TypeInfo>())
            {
                foreach (var implementedInterface in typeInfo.ImplementedInterfaces)
                {
                    services.AddScoped(implementedInterface, typeInfo);
                }
            }

            services.AddScoped<IMapper, DefaultMapper>();

            return services;
        }
    }
}
