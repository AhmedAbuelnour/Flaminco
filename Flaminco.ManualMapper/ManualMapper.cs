using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.ManualMapper
{
    public class ManualMapper<TDestination> : IManualMapper<TDestination>
    {
        private readonly IServiceProvider _serviceProvider;
        public ManualMapper(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public ValueTask<TDestination> Map<TMapProfile>(TMapProfile profile, Action<MapperOptions>? options = null, CancellationToken cancellationToken = default) where TMapProfile : IMapProfile<TDestination>
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            IMapProfileHandler<TMapProfile, TDestination>? handler = _serviceProvider.GetService<IMapProfileHandler<TMapProfile, TDestination>>();

            if (handler is null)
            {
                throw new InvalidOperationException($"{nameof(IMapProfileHandler<TMapProfile, TDestination>)} Is not registed as a service");
            }
            return handler.Handler(profile, options, cancellationToken);
        }
    }
}
