namespace Flaminco.ManualMapper
{
    public interface IManualMapper
    {
        ValueTask<TDestination> Map<TSource, TDestination>(IMapHandler<TSource, TDestination> handler, TSource source, Action<MapperOptions>? options = default, CancellationToken cancellationToken = default);
    }
}
