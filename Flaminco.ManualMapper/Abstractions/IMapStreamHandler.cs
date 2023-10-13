namespace Flaminco.ManualMapper.Abstractions
{
    public interface IMapStreamHandler<TSource, TDestination>
    {
        IAsyncEnumerable<TDestination> Handler(TSource source, CancellationToken cancellationToken = default);
    }
}
