using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.ManualMapper
{
    public static class ManualMapperExtension
    {
        public static IServiceCollection AddManualMapper(this IServiceCollection services, Type assemblyScanner)
        {
            services.Scan(scan =>
               scan.FromAssembliesOf(assemblyScanner)
                 .AddClasses(classes =>
                 classes.AssignableTo(typeof(IMapProfileHandler<,>)).Where(x => !x.IsGenericType))
                     .AsImplementedInterfaces()
                     .WithTransientLifetime());

            services.AddSingleton(typeof(IManualMapper<>), typeof(ManualMapper<>));

            return services;
        }
    }
}
