using Flaminco.ManualMapper.Abstractions;

namespace Flaminco.ManualMapper.Implementations;

public sealed class DefaultManualMapper : IManualMapper
{
    private readonly IServiceProvider _serviceProvider;
    public DefaultManualMapper(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public ValueTask<TResponse> Map<TResponse>(IMapProfile<TResponse> profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        Type profileType = typeof(IMapProfileHandler<,>).MakeGenericType(profile.GetType(), typeof(TResponse));

        object? handler = _serviceProvider.GetService(profileType);

        ArgumentNullException.ThrowIfNull(handler);

        if (handler.GetType().GetMethod("Handler")?.Invoke(handler, new object[] { profile, cancellationToken }) is ValueTask<TResponse> profileMapper)
        {
            return profileMapper;
        }
        else
        {
            throw new InvalidOperationException($"{nameof(handler)} is not registered");
        }
    }
}
