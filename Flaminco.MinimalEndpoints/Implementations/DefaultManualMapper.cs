using Flaminco.MinimalEndpoints.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.MinimalEndpoints.Implementations;

internal sealed class DefaultManualMapper(IServiceProvider _serviceProvider) : IMapper
{
    public ValueTask<TDestination?> Map<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        IMapHandler<TSource, TDestination> handler = _serviceProvider.GetRequiredService<IMapHandler<TSource, TDestination>>();

        return handler.Handler(source, cancellationToken);
    }
}