namespace Flaminco.ManualMapper.Abstractions;

public interface IMapper
{
    Task<TDestination> MapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default) where TDestination : class;
    TDestination Map<TSource, TDestination>(TSource source) where TDestination : class;
    IAsyncEnumerable<TDestination> MapStream<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default) where TDestination : class;
}
