using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.PeriodicAction;

public static class PeriodicActionExtension
{
    public static IServiceCollection AddPeriodicAction(this IServiceCollection services, Action<PeriodicActionOption> options)
    {
        services.Configure(options);
        services.AddSingleton<PeriodicActionHandler>();
        return services;
    }
}