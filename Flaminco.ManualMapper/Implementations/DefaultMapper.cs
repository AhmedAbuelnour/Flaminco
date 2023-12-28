using Flaminco.ManualMapper.Abstractions;
using Flaminco.ManualMapper.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.ManualMapper.Implementations;

public sealed class DefaultMapper(IServiceProvider serviceProvider) : IMapper
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public TDestination Map<TSource, TDestination>(TSource source) where TDestination : class
    {
        ArgumentNullException.ThrowIfNull(source);

        IMapHandler<TSource, TDestination>? handler = _serviceProvider.GetService<IMapHandler<TSource, TDestination>>();

        return handler switch
        {
            null => throw new HandlerNotFoundException<TSource, TDestination>(),
            _ => handler.Handler(source)
        };
    }

    public Task<TDestination> MapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default) where TDestination : class
    {
        ArgumentNullException.ThrowIfNull(source);

        IMapAsyncHandler<TSource, TDestination>? handler = _serviceProvider.GetService<IMapAsyncHandler<TSource, TDestination>>();

        return handler switch
        {
            null => throw new HandlerNotFoundException<TSource, TDestination>(),
            _ => handler.Handler(source, cancellationToken)
        };
    }

    public IAsyncEnumerable<TDestination> MapStream<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default) where TDestination : class
    {
        ArgumentNullException.ThrowIfNull(source);

        IMapStreamHandler<TSource, TDestination>? handler = _serviceProvider.GetService<IMapStreamHandler<TSource, TDestination>>();

        return handler switch
        {
            null => throw new HandlerNotFoundException<TSource, TDestination>(),
            _ => handler.Handler(source, cancellationToken)
        };
    }
}
