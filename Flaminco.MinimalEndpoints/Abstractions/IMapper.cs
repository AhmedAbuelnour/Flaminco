namespace Flaminco.MinimalEndpoints.Abstractions;

using System.Threading;
/// <summary>
/// Defines a contract for mapping objects of one type to another.
/// </summary>
public interface IMapper
{
    /// <summary>
    /// Maps an object of type <typeparamref name="TSource"/> to an object of type <typeparamref name="TDestination"/>.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source">The source object to map from.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the mapped object of type <typeparamref name="TDestination"/>.</returns>
    Task<TDestination> MapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default);

    /// <summary>
    /// Maps an object of type <typeparamref name="TSource"/> to an object of type <typeparamref name="TDestination"/>.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source">The source object to map from.</param>
    /// <returns>The result contains the mapped object of type <typeparamref name="TDestination"/>.</returns>
    TDestination Map<TSource, TDestination>(TSource source) where TDestination : class;
}

/// <summary>
/// Defines a contract for handling the mapping logic between a source and destination type.
/// </summary>
/// <typeparam name="TSource">The source type.</typeparam>
/// <typeparam name="TDestination">The destination type.</typeparam>
public interface IMapHandler<in TSource, out TDestination>
{
    /// <summary>
    /// Handles the mapping logic for converting an object of type <typeparamref name="TSource"/> to an object of type <typeparamref name="TDestination"/>.
    /// </summary>
    /// <param name="source">The source object to map from.</param>
    /// <returns> The result contains the mapped object of type <typeparamref name="TDestination"/>.</returns>
    TDestination Handler(TSource source);
}


/// <summary>
/// Defines a contract for handling the mapping logic between a source and destination type.
/// </summary>
/// <typeparam name="TSource">The source type.</typeparam>
/// <typeparam name="TDestination">The destination type.</typeparam>
public interface IMapAsyncHandler<in TSource, TDestination>
{
    /// <summary>
    /// Handles the mapping logic for converting an object of type <typeparamref name="TSource"/> to an object of type <typeparamref name="TDestination"/>.
    /// </summary>
    /// <param name="source">The source object to map from.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the mapped object of type <typeparamref name="TDestination"/>.</returns>
    Task<TDestination> Handler(TSource source, CancellationToken cancellationToken = default);
}
