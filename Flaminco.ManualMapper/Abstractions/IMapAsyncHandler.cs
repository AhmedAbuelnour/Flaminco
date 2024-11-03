namespace Flaminco.ManualMapper.Abstractions;

public interface IMapAsyncHandler<TSource, TDestination>
{
    Task<TDestination> Handler(TSource source, CancellationToken cancellationToken = default);
}