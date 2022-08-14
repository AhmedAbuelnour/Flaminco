namespace Flaminco.ManualMapper
{
    public interface IManualMapper<TDestination>
    {
        ValueTask<TDestination> Map<TMapProfile>(TMapProfile profile, Action<MapperOptions>? options = default, CancellationToken cancellationToken = default)
            where TMapProfile : IMapProfile<TDestination>;
    }
}
