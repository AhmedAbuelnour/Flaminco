namespace Flaminco.ManualMapper.Abstractions;

public interface IMapper
{
    Task<TDestination> MapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default);
    TDestination Map<TSource, TDestination>(TSource source);
    IAsyncEnumerable<TDestination> MapStream<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default);
}
