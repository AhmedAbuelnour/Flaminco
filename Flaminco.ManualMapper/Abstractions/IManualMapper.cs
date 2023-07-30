namespace Flaminco.ManualMapper.Abstractions;

public interface IManualMapper
{
    Task<TDestination> MapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default);
    TDestination Map<TSource, TDestination>(TSource source);
}
