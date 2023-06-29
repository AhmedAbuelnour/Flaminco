namespace Flaminco.ManualMapper.Abstractions;

public interface IMapProfileHandler<in TMapProfile, TResponse> where TMapProfile : IMapProfile<TResponse>
{
    ValueTask<TResponse> Handler(TMapProfile profile, CancellationToken cancellationToken = default);
}
