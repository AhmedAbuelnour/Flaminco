﻿using Flaminco.Pipeline.Abstractions;
using Flaminco.Pipeline.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.Pipeline.Implementations;

public sealed class DefaultPipeline : IPipeline
{
    private readonly IServiceProvider _serviceProvider;

    public DefaultPipeline(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async ValueTask ExecutePipeline<TInput>(TInput source, CancellationToken cancellationToken = default)
        where TInput : class
    {
        var handlers = _serviceProvider.GetServices<IPipelineHandler<TInput>>();

        if (!handlers?.Any() ?? true) throw new InvalidOperationException("No pipeline are registered!");

        PriorityQueue<IPipelineHandler<TInput>, int> handlerQueue = new();

        foreach (var handler in handlers!)
            if (Attribute.GetCustomAttribute(handler.GetType(), typeof(PipelineAttribute<TInput>)) is
                PipelineAttribute<TInput> attribute)
                handlerQueue.Enqueue(handler, attribute.Order);

        while (handlerQueue.TryDequeue(out var handler, out _)) await handler.Handler(source, cancellationToken);
    }

    public async ValueTask ExecuteKeyedPipeline<TInput>(TInput source, string key,
        CancellationToken cancellationToken = default) where TInput : class
    {
        var handlers = _serviceProvider.GetServices<IPipelineHandler<TInput>>();

        if (!handlers?.Any() ?? true) throw new InvalidOperationException("No pipeline are registered!");

        PriorityQueue<IPipelineHandler<TInput>, int> handlerQueue = new();

        foreach (var handler in handlers!)
            if (Attribute.GetCustomAttributes(handler.GetType(), typeof(KeyedPipelineAttribute<TInput>)) is
                    KeyedPipelineAttribute<TInput>[] attributes &&
                attributes.FirstOrDefault(a => a.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase)) is
                    KeyedPipelineAttribute<TInput> attribute)
                handlerQueue.Enqueue(handler, attribute.Order);

        while (handlerQueue.TryDequeue(out var handler, out _)) await handler.Handler(source, cancellationToken);
    }
}