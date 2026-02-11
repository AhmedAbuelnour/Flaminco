using Flaminco.MinimalEndpoints.Abstractions;
using Microsoft.AspNetCore.Routing;
using System.Reflection;

namespace Flaminco.MinimalEndpoints.Extensions;


/// <summary>
/// Provides extension methods for working with endpoint modules.
/// </summary>
public static class ModuleExtension
{
    /// <summary>
    /// Maps the routes for all registered modules to the endpoint route builder.
    /// </summary>
    /// <param name="builder">The endpoint route builder to map the module routes to.</param>
    /// <returns>The updated endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapModules<TModuleScanner>(this IEndpointRouteBuilder builder)
    {
        static IEnumerable<TypeInfo> enumerable()
        {
            foreach (var type in typeof(TModuleScanner).Assembly.DefinedTypes)
            {
                if (!type.IsAbstract && typeof(IModule).IsAssignableFrom(type) && type != typeof(IModule) && type.IsPublic)
                {
                    yield return type;
                }
            }
        }

        foreach (var moduleType in enumerable() ?? [])
        {
            var method = moduleType.GetMethod(nameof(IModule.AddRoutes), BindingFlags.Public | BindingFlags.Static);

            if (method?.GetParameters() is [var param] && param.ParameterType == typeof(IEndpointRouteBuilder))
            {
                try
                {
                    method.Invoke(null, [builder]);
                }
                catch (Exception ex)
                {
                    // Log or handle module registration failures
                    throw new InvalidOperationException($"Failed to register module {moduleType.Name}", ex);
                }
            }
        }

        return builder;
    }

    public static IEndpointRouteBuilder MapModules(this IEndpointRouteBuilder builder, params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            throw new ArgumentNullException(nameof(assemblies));
        }

        static IEnumerable<TypeInfo> enumerable(Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.DefinedTypes)
                {
                    if (!type.IsAbstract && typeof(IModule).IsAssignableFrom(type) && type != typeof(IModule) && type.IsPublic)
                    {
                        yield return type;
                    }
                }
            }
        }

        foreach (var moduleType in enumerable(assemblies) ?? [])
        {
            var method = moduleType.GetMethod(nameof(IModule.AddRoutes), BindingFlags.Public | BindingFlags.Static);
            if (method?.GetParameters() is [var param] && param.ParameterType == typeof(IEndpointRouteBuilder))
            {
                try
                {
                    method.Invoke(null, [builder]);
                }
                catch (Exception ex)
                {
                    // Log or handle module registration failures
                    throw new InvalidOperationException($"Failed to register module {moduleType.Name}", ex);
                }
            }
        }
        return builder;
    }
}