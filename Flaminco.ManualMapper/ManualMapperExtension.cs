using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.ManualMapper
{
    public static class ManualMapperExtension
    {
        public static IServiceCollection AddManualMapper(this IServiceCollection services)
        {
            services.AddSingleton<IManualMapper, ManualMapper>();

            return services;
        }
    }
}
