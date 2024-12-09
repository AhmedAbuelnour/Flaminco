using Flaminco.MinimalMediatR.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.MinimalMediatR.Extensions;


public static class ModuleExtension
{
    /// <summary>
    /// Adds all implementations of the IModule interface from the specified assembly to the service collection.
    /// </summary>
    /// <typeparam name="TScanner">The type used to locate the assembly to scan for modules.</typeparam>
    /// <param name="services">The service collection to add the modules to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddModules<TScanner>(this IServiceCollection services)
    {
        IEnumerable<Type>? types = from type in typeof(TScanner).Assembly.DefinedTypes
                                   where !type.IsAbstract && typeof(IModule).IsAssignableFrom(type) && type != typeof(IModule) && type.IsPublic
                                   select type;

        foreach (var type in types ?? []) services.AddSingleton(typeof(IModule), type);

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