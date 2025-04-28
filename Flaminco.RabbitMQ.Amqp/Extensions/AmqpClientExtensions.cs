namespace Flaminco.RabbitMQ.AMQP.Extensions
{
    using Flaminco.RabbitMQ.AMQP.Abstractions;
    using Flaminco.RabbitMQ.AMQP.Attributes;
    using Flaminco.RabbitMQ.AMQP.HostedServices;
    using Flaminco.RabbitMQ.AMQP.Services;
    using global::RabbitMQ.Client;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using System;
    using System.Reflection;

    /// <summary>
    /// Provides extension methods for configuring AMQP clients and registering publishers and consumers in the service collection.
    /// </summary>
    public static class AmqpClientExtensions
    {
        /// <summary>
        /// Configures the AMQP client by registering the connection provider,
        /// publishers, and consumers found in the given assemblies.
        /// </summary>
        public static IServiceCollection AddAmqpClient(this IServiceCollection services, Action<ConnectionFactory> clientSettings, params Assembly[] scannerAssemblies)
        {
            // 1. Bind settings
            services.Configure(clientSettings);

            // 2. Core connection provider
            services.AddSingleton<AmqpConnectionProvider>();

            // 3. Publishers & Consumers from all supplied assemblies
            services.AddPublishers(scannerAssemblies).AddConsumers(scannerAssemblies);

            // 4. Hosted service for consumer lifecycle
            services.AddSingleton<IHostedService, AmqpConsumerHostedService>();

            // 5. Health check against the AMQP connection

            services.AddHealthChecks().AddRabbitMQ(async sp =>
            {
                return await sp.GetRequiredService<AmqpConnectionProvider>().GetConnectionAsync(CancellationToken.None);
            });

            return services;
        }


        /// <summary>
        /// Scans the provided assemblies for any types deriving from MessagePublisher
        /// (excluding abstract types) and registers them as scoped.
        /// </summary>
        private static IServiceCollection AddPublishers(this IServiceCollection services, params Assembly[] scannerAssemblies)
        {
            foreach (TypeInfo pub in scannerAssemblies.SelectMany(a => a.DefinedTypes).Where(t => t.IsSubclassOf(typeof(MessagePublisher)) && !t.IsAbstract))
                services.AddScoped(pub.AsType());

            return services;
        }

        /// <summary>
        /// Scans the provided assemblies for any types decorated with QueueConsumerAttribute
        /// and registers them as transient.
        /// </summary>
        private static IServiceCollection AddConsumers(this IServiceCollection services, params Assembly[] scannerAssemblies)
        {
            foreach (Type consumer in scannerAssemblies.SelectMany(a => a.GetTypes()).Where(t => t.GetCustomAttributes<QueueConsumerAttribute>().Any()))
                services.AddTransient(consumer);

            return services;
        }
    }
}
