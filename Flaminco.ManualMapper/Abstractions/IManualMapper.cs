namespace Flaminco.ManualMapper.Abstractions
{
    public interface IManualMapper
    {
        ValueTask<TResponse> Map<TResponse>(IMapProfile<TResponse> profile, CancellationToken cancellationToken = default);
    }
}
