﻿using System.Reflection;
using Flaminco.Pipeline.Abstractions;
using Flaminco.Pipeline.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.Pipeline.Extensions;

public static class PipelineExtension
{
    public static IServiceCollection AddPipelines<T>(this IServiceCollection services)
    {
        IEnumerable<TypeInfo>? types = from type in typeof(T).Assembly.DefinedTypes
            where !type.IsAbstract && type.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineHandler<>))
            select type.GetTypeInfo();

        foreach (var typeInfo in types ?? Array.Empty<TypeInfo>())
        foreach (var implementedInterface in typeInfo.ImplementedInterfaces)
            services.AddScoped(implementedInterface, typeInfo);

        services.AddScoped<IPipeline, DefaultPipeline>();

        return services;
    }
}