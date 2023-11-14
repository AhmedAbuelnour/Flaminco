using Flaminco.ManualMapper.Abstractions;
using Flaminco.ManualMapper.Implementations;
using Flaminco.Validation.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Flaminco.ManualMapper.Extensions;

public static class ValidationExtension
{
    public static IServiceCollection AddValidation<T>(this IServiceCollection services)
    {
        IEnumerable<TypeInfo>? types = from type in typeof(T).Assembly.DefinedTypes
                                       where !type.IsAbstract && type.GetInterfaces().Any(i => i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(IValidationHandler<>) || i.GetGenericTypeDefinition() == typeof(IValidationAsyncHandler<>)))
                                       select type.GetTypeInfo();

        foreach (TypeInfo? typeInfo in types ?? Array.Empty<TypeInfo>())
        {
            foreach (var implementedInterface in typeInfo.ImplementedInterfaces)
            {
                services.AddScoped(implementedInterface, typeInfo);
            }
        }

        services.AddScoped<IValidation, DefaultValidation>();

        return services;
    }
}
