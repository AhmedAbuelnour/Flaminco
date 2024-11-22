namespace Flaminco.ManualMapper.Abstractions;

public interface IMapHandler<TSource, TDestination>
{
    TDestination? Handler(TSource source);
}