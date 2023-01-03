using Flaminco.ManualMapper.Abstractions;

namespace Flaminco.ManualMapper.Implementations;

public sealed class DefaultManualMapper : IManualMapper
{
    private readonly IServiceProvider _serviceProvider;
    public DefaultManualMapper(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async ValueTask<TResponse> Map<TResponse>(IMapProfile<TResponse> profile, string[]? args = default, CancellationToken cancellationToken = default)
    {
        if (profile == null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        Type profileType = typeof(IMapProfileHandler<,>).MakeGenericType(profile.GetType(), typeof(TResponse));

        object? handler = _serviceProvider.GetService(profileType);

        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        if (handler.GetType().GetMethod("Handler")?.Invoke(handler, new object[] { profile, args, cancellationToken }) is ValueTask<TResponse> profileMapper)
        {
            return await profileMapper;
        }
        else
        {
            throw new InvalidOperationException($"{nameof(handler)} is not registered");
        }
    }
}
