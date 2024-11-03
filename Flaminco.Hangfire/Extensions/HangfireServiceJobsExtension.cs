using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Flaminco.Hangfire.Abstractions;
using Flaminco.Hangfire.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.Hangfire.Extensions;

public static class HangfireServiceLocatorExtension
{
    public static IServiceCollection AddHangfireServiceLocator<TAssemblyScanner>(this IServiceCollection services)
    {
        IEnumerable<TypeInfo> types = from type in typeof(TAssemblyScanner).Assembly.DefinedTypes
            where !type.IsAbstract && typeof(IServiceJob).IsAssignableFrom(type)
            select type.GetTypeInfo();

        foreach (Type? type in types) services.AddScoped(typeof(IServiceJob), type);

        services.AddScoped<IHangfireServiceLocator, DefaultHangfireServiceLocator>();

        return services;
    }
}