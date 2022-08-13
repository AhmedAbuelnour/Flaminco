namespace Flaminco.ManualMapper
{
    public interface IMapHandler<TSource, TDestination>
    {
        ValueTask<TDestination> Handler(TSource source, Action<MapperOptions>? options = default, CancellationToken cancellationToken = default);
    }
}