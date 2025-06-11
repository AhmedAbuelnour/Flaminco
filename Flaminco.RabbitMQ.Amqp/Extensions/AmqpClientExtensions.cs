using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Flaminco.RabbitMQ.AMQP.Attributes;

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
                    cfg.UseRawJsonSerializer();
                    cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

                    foreach (Type consumer in scannerAssemblies
                        .SelectMany(a => a.GetTypes())
                        .Where(t => t.IsClass && !t.IsAbstract && t.GetCustomAttribute<QueueConsumerAttribute>() != null))
                    {
                        QueueConsumerAttribute attribute = consumer.GetCustomAttribute<QueueConsumerAttribute>()!;
                        cfg.ReceiveEndpoint(attribute.Queue, e => e.ConfigureConsumer(context, consumer));
                    }

                    cfg.ConfigureEndpoints(context);

                    busConfiguration?.Invoke(cfg);
                });
            });

            // Health check
            services.AddHealthChecks().AddRabbitMQ();

            return services;
        }
    }
}
