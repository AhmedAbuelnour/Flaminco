using Flaminco.MinimalEndpoints.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.MinimalEndpoints.Implementations;

/// <summary>
/// Provides a default implementation of the <see cref="IMapper"/> interface.
/// This implementation uses dependency injection to resolve the appropriate <see cref="IMapHandler{TSource, TDestination}"/>
/// for mapping objects from a source type to a destination type.
/// </summary>
internal sealed class DefaultManualMapper(IServiceProvider _serviceProvider) : IMapper
{
    /// <summary>
    /// Maps an object of type <typeparamref name="TSource"/> to an object of type <typeparamref name="TDestination"/>.
    /// </summary>
    /// <typeparam name="TSource">The source type of the object to map.</typeparam>
    /// <typeparam name="TDestination">The destination type of the object to map to.</typeparam>
    /// <param name="source">The source object to map.</param>
    /// <returns>The mapped object of type <typeparamref name="TDestination"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="source"/> is null.</exception>
    public TDestination Map<TSource, TDestination>(TSource source) where TDestination : class
    {
        ArgumentNullException.ThrowIfNull(source);

        IMapHandler<TSource, TDestination> handler = _serviceProvider.GetRequiredService<IMapHandler<TSource, TDestination>>();

        return handler.Handler(source);
    }

    /// <summary>
    /// Maps an object of type <typeparamref name="TSource"/> to an object of type <typeparamref name="TDestination"/>.
    /// </summary>
    /// <typeparam name="TSource">The source type of the object to map.</typeparam>
    /// <typeparam name="TDestination">The destination type of the object to map to.</typeparam>
    /// <param name="source">The source object to map.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing the mapped object of type <typeparamref name="TDestination"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="source"/> is null.</exception>
    public Task<TDestination> MapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        IMapAsyncHandler<TSource, TDestination> handler = _serviceProvider.GetRequiredService<IMapAsyncHandler<TSource, TDestination>>();

        return handler.Handler(source, cancellationToken);
    }
}
