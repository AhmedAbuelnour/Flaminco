namespace Flaminco.MinimalEndpoints.Abstractions;

public interface IMapper
{
    ValueTask<TDestination?> Map<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default);
}

public interface IMapHandler<TSource, TDestination>
{
    ValueTask<TDestination?> Handler(TSource source, CancellationToken cancellationToken = default);
}