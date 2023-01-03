using Flaminco.Pipeline.Abstractions;
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

    public async ValueTask ExecutePipeline<TValue>(TValue source, string name, CancellationToken cancellationToken = default) where TValue : class
    {
        IEnumerable<IPipelineHandler<TValue>>? handlers = _serviceProvider.GetServices<IPipelineHandler<TValue>>();

        if (!handlers?.Any() ?? true)
        {
            throw new InvalidOperationException("No pipeline are registered!");
        }

        PriorityQueue<IPipelineHandler<TValue>, int> handlerQueue = new();

        foreach (IPipelineHandler<TValue> handler in handlers!)
        {
            if (Attribute.GetCustomAttribute(handler.GetType(), typeof(PipelineAttribute)) is PipelineAttribute attribute && attribute.Name == name)
            {
                handlerQueue.Enqueue(handler, attribute.Order);
            }
        }

        while (handlerQueue.TryDequeue(out IPipelineHandler<TValue>? handler, out _))
        {
            await handler.Handler(source, cancellationToken);
        }
    }
}
