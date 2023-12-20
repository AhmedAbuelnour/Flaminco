using Flaminco.MinimalMediatR.Cached.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.MinimalMediatR.Cached.Extensions
{
    public static class MinimalMediatRCachedExtensions
    {
        public static IServiceCollection AddMinimalMediatR<TScanner>(this IServiceCollection services)
        {
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(typeof(TScanner).Assembly);

                config.AddOpenBehavior(typeof(CachedEndPointPipelineBehavior<,>));
            });

            services.AddMemoryCache();

            return services;
        }
    }
}
