namespace Flaminco.AzureBus.AMQP.Extensions
{
    using Flaminco.AzureBus.AMQP.Abstractions;
    using Flaminco.AzureBus.AMQP.Attributes;
    using Flaminco.AzureBus.AMQP.HealthChecks;
    using Flaminco.AzureBus.AMQP.HostedServices;
    using Flaminco.AzureBus.AMQP.Models;
    using Flaminco.AzureBus.AMQP.Services;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Hosting;
    using System;
    using System.Reflection;

    /// <summary>
    /// Provides extension methods for configuring AMQP clients and registering publishers and consumers in the service collection.
    /// </summary>
    public static class AmqpClientExtensions
    {
        /// <summary>
        /// Configures the AMQP client by registering the AMQP locator, publishers, and consumers in the service collection.
        /// </summary>
        /// <typeparam name="TScanner">A type from the assembly to scan for message publisher implementations or consumer implementations.</typeparam>
        /// <param name="services">The service collection to which the AMQP client, publishers, and consumers will be added.</param>
        /// <param name="clientSettings">The settings used to configure the AMQP client.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddAmqpClient<TScanner>(this IServiceCollection services, Action<AmqpClientSettings> clientSettings)
        {
            // Configure settings
            services.Configure(clientSettings);

            // Add core services
            services.AddSingleton<AmqpConnectionProvider>();

            // Add publishers and consumers
            services.AddPublishers<TScanner>();

            services.AddConsumers<TScanner>();

            // Add hosted service to manage consumer lifecycle
            services.AddSingleton<IHostedService, AmqpConsumerHostedService>();

            return services;
        }

        /// <summary>
        /// Configures the AMQP client with health checks enabled.
        /// </summary>
        /// <param name="services">The service collection to which the AMQP client, publishers, and consumers will be added.</param>
        /// <param name="healthCheckName">Optional name for the health check. Defaults to "amqp".</param>
        /// <param name="failureStatus">Optional failure status for the health check. Defaults to HealthStatus.Unhealthy.</param>
        /// <param name="tags">Optional tags for the health check.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddAmqpHealthChecks(this IServiceCollection services,
                                                             string healthCheckName = "amqp",
                                                             HealthStatus failureStatus = HealthStatus.Unhealthy,
                                                             IEnumerable<string>? tags = null)
        {
            // Add health check
            services.AddHealthChecks().AddCheck<AmqpConnectionHealthCheck>(name: healthCheckName, failureStatus: failureStatus, tags: tags ?? ["amqp", "messaging", "rabbitmq"]);

            return services;
        }

        /// <summary>
        /// Adds consumer registrations to the service collection for the specified scanner type.
        /// </summary>
        /// <typeparam name="TScanner">A type used to scan for consumer implementations.</typeparam>
        /// <param name="services">The service collection to which the consumers will be added.</param>
        /// <returns>The updated service collection.</returns>
        private static IServiceCollection AddConsumers<TScanner>(this IServiceCollection services)
        {
            var consumerTypes = typeof(TScanner).Assembly.GetTypes()
                .Where(t => t.GetCustomAttributes<QueueConsumerAttribute>().Any())
                .ToList();

            foreach (var consumerType in consumerTypes)
            {
                services.AddTransient(consumerType);
            }

            return services;
        }

        /// <summary>
        /// Adds publisher registrations to the service collection for the specified scanner type.
        /// </summary>
        /// <typeparam name="TScanner">A type used to scan for publisher implementations.</typeparam>
        /// <param name="services">The service collection to which the publishers will be added.</param>
        private static void AddPublishers<TScanner>(this IServiceCollection services)
        {
            foreach (var type in typeof(TScanner).Assembly.DefinedTypes
                .Where(type => type.IsSubclassOf(typeof(MessagePublisher)) && !type.IsAbstract))
            {
                services.AddScoped(type);
            }
        }
    }
}
