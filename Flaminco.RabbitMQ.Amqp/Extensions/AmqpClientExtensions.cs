using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.RabbitMQ.AMQP.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring MassTransit with RabbitMQ.
    /// </summary>
    public static class AmqpClientExtensions
    {
        /// <summary>
        /// Adds MassTransit configured with RabbitMQ.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="register">Optional registration action for consumers and other components.</param>
        /// <param name="busConfiguration">Optional configuration action for the RabbitMQ bus.</param>
        public static IServiceCollection AddAmqpClient(this IServiceCollection services,
                                                       Action<IBusRegistrationConfigurator>? register = null,
                                                       Action<IRabbitMqBusFactoryConfigurator, IBusRegistrationContext>? busConfiguration = null)
        {
            services.AddMassTransit(x =>
            {
                register?.Invoke(x);

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.UseRawJsonSerializer();
                    cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                    busConfiguration?.Invoke(cfg, context);

                    cfg.ConfigureEndpoints(context);
                });
            });

            // Health check
            services.AddHealthChecks().AddRabbitMQ();

            return services;
        }
    }
}
