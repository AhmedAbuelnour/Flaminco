namespace Flaminco.ManualMapper;

public interface IMapProfileHandler<TMapProfile, TDestination> where TMapProfile : IMapProfile<TDestination>
{
    ValueTask<TDestination> Handler(TMapProfile profile, Action<MapperOptions>? options = default, CancellationToken cancellationToken = default);
}
