using Flaminco.ManualMapper.Abstractions;

namespace Flaminco.ManualMapper.Implementations;

public sealed class DefaultMapper : IMapper
{
    private readonly IServiceProvider _serviceProvider;
    public DefaultMapper(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public TDestination Map<TSource, TDestination>(TSource source)
    {
        ArgumentNullException.ThrowIfNull(source);

        Type profileType = typeof(IMapHandler<,>).MakeGenericType(typeof(TSource), typeof(TDestination));

        object? handler = _serviceProvider.GetService(profileType);

        ArgumentNullException.ThrowIfNull(handler);

        return handler.GetType().GetMethod("Handler")?.Invoke(handler, new object[] { source }) switch
        {
            TDestination destination => destination,
            _ => throw new InvalidOperationException($"{nameof(handler)} is not registered")
        };
    }

    public Task<TDestination> MapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        Type profileType = typeof(IMapAsyncHandler<,>).MakeGenericType(typeof(TSource), typeof(TDestination));

        object? handler = _serviceProvider.GetService(profileType);

        ArgumentNullException.ThrowIfNull(handler);

        return handler.GetType().GetMethod("Handler")?.Invoke(handler, new object[] { source, cancellationToken }) switch
        {
            Task<TDestination> destination => destination,
            _ => throw new InvalidOperationException($"{nameof(handler)} is not registered")
        };
    }

    public IAsyncEnumerable<TDestination> MapStream<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        Type profileType = typeof(IMapStreamHandler<,>).MakeGenericType(typeof(TSource), typeof(TDestination));

        object? handler = _serviceProvider.GetService(profileType);

        ArgumentNullException.ThrowIfNull(handler);

        return handler.GetType().GetMethod("Handler")?.Invoke(handler, new object[] { source, cancellationToken }) switch
        {
            IAsyncEnumerable<TDestination> destination => destination,
            _ => throw new InvalidOperationException($"{nameof(handler)} is not registered")
        };
    }

}
