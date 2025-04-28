using Flaminco.MinimalEndpoints.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.MinimalEndpoints.Extensions;


/// <summary>
/// Provides extension methods for working with endpoint modules.
/// </summary>
public static class ModuleExtension
{
    /// <summary>
    /// Adds all implementations of the IModule interface from the specified assembly to the service collection.
    /// </summary>
    /// <typeparam name="TModuleScanner">The type used to locate the assembly to scan for modules.</typeparam>
    /// <param name="services">The service collection to add the modules to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddModules<TModuleScanner>(this IServiceCollection services)
    {
        static IEnumerable<System.Reflection.TypeInfo> enumerable()
        {
            foreach (var type in typeof(TModuleScanner).Assembly.DefinedTypes)
            {
                if (!type.IsAbstract && typeof(IModule).IsAssignableFrom(type) && type != typeof(IModule) && type.IsPublic)
                {
                    yield return type;
                }
            }
        }

        foreach (var type in enumerable() ?? []) services.AddSingleton(typeof(IModule), type);

        return services;
    }

    /// <summary>
    /// Adds all implementations of the IMinimalRouteEndpoint interface from the specified assembly to the service collection.
    /// </summary>
    /// <typeparam name="TEndpointScanner">The type used to locate the assembly to scan for endpoints.</typeparam>
    /// <param name="services">The service collection to add the endpoints to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddEndpoints<TEndpointScanner>(this IServiceCollection services)
    {
        foreach (Type? endpoint in typeof(TEndpointScanner).Assembly.GetTypes()
                                                                  .Where(t => t.GetInterfaces().Contains(typeof(IMinimalRouteEndpoint)))
                                                                  .Where(t => !t.IsInterface))
        {
            services.AddScoped(endpoint);
        }

        return services;
    }

    /// <summary>
    /// Maps the routes for all registered modules to the endpoint route builder.
    /// </summary>
    /// <param name="builder">The endpoint route builder to map the module routes to.</param>
    /// <returns>The updated endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapModules(this IEndpointRouteBuilder builder)
    {
        foreach (var module in builder.ServiceProvider.GetServices<IModule>()) module.AddRoutes(builder);

        return builder;
    }
}