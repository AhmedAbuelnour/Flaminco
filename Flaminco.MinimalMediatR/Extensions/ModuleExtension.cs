using Flaminco.MinimalMediatR.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.MinimalMediatR.Extensions;

public static class ModuleExtension
{
    public static IServiceCollection AddModules<TScanner>(this IServiceCollection services)
    {
        IEnumerable<Type>? types = from type in typeof(TScanner).Assembly.DefinedTypes
                                   where !type.IsAbstract && typeof(IModule).IsAssignableFrom(type) && type != typeof(IModule) && type.IsPublic
                                   select type;

        foreach (Type? type in types ?? Array.Empty<Type>())
        {
            services.AddSingleton(typeof(IModule), type);
        }

        return services;
    }

    public static IEndpointRouteBuilder MapModules(this IEndpointRouteBuilder builder)
    {
        foreach (IModule module in builder.ServiceProvider.GetServices<IModule>())
        {
            module.AddRoutes(builder);
        }

        return builder;
    }
}
