using Flaminco.MinimalEndpoints.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Flaminco.MinimalEndpoints.Extensions
{
    public static class ValidatorExtension
    {
        /// <summary>
        /// Adds all implementations of the IValidator interface from the specified assembly to the service collection.
        /// </summary>
        /// <typeparam name="TScanner">The type used to locate the assembly to scan for validators.</typeparam>
        /// <param name="services">The service collection to add the validators to.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddValidators<TScanner>(this IServiceCollection services)
        {
            IEnumerable<TypeInfo>? types = from type in typeof(TScanner).Assembly.DefinedTypes
                                           where !type.IsAbstract && type.GetInterfaces().Any(i =>
                                               i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(IMinimalValidator<>) || i.GetGenericTypeDefinition() == typeof(IAsyncMinimalValidator<>)))
                                           select type.GetTypeInfo();

            foreach (var (typeInfo, implementedInterface) in (types ?? []).SelectMany(typeInfo => typeInfo.ImplementedInterfaces.Select(implementedInterface => (typeInfo, implementedInterface))))
            {
                services.AddTransient(implementedInterface, typeInfo);
            }


            return services;
        }
    }
}
