namespace Flaminco.ManualMapper
{
    public interface IManualMapper
    {
        ValueTask<TDestination> Map<TMapProfile, TDestination>(TMapProfile profile, Action<MapperOptions>? options = default, CancellationToken cancellationToken = default)
            where TMapProfile : IMapProfile<TDestination>;
    }
}
