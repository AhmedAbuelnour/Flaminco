using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Flaminco.RabbitMQ.AMQP.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring MassTransit with RabbitMQ.
    /// </summary>
    public static class AmqpClientExtensions
    {
        /// <summary>
        /// Adds MassTransit configured with RabbitMQ and registers all consumers found in the provided assemblies.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="busConfiguration">Optional configuration action for the RabbitMQ bus.</param>
        /// <param name="scannerAssemblies">Assemblies to scan for consumers.</param>
        public static IServiceCollection AddAmqpClient(this IServiceCollection services,
                                                       Action<IRabbitMqBusFactoryConfigurator>? busConfiguration,
                                                       params Assembly[] scannerAssemblies)
        {
            services.AddMassTransit(x =>
            {
                foreach (Assembly assembly in scannerAssemblies)
                    x.AddConsumers(assembly);

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                    cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

                    busConfiguration?.Invoke(cfg);
                });
            });

            // Health check
            services.AddHealthChecks().AddRabbitMQ();

            return services;
        }
    }
}
