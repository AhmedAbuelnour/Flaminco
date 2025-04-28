namespace Flaminco.RabbitMQ.AMQP.Extensions
{
    using Azure.Messaging.ServiceBus;
    using Flaminco.AzureBus.AMQP.Abstractions;
    using Flaminco.AzureBus.AMQP.Attributes;
    using Flaminco.AzureBus.AMQP.HostedServices;
    using Flaminco.AzureBus.AMQP.Services;
    using Microsoft.Extensions.Azure;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Reflection;

    /// <summary>
    /// Provides extension methods for configuring Azure Service Bus AMQP clients and registering publishers and consumers in the dependency injection container.
    /// </summary>
    /// <remarks>
    /// This static class contains extension methods that simplify the registration and configuration of Azure Service Bus
    /// components in an ASP.NET Core or .NET application that uses dependency injection.
    /// </remarks>
    public static class AmqpClientExtensions
    {
        /// <summary>
        /// Configures and registers all Azure Service Bus components in the application's dependency injection container.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="connectionString">The connection string for the Azure Service Bus namespace.</param>
        /// <param name="clientSettings">An action to configure additional Service Bus client options.</param>
        /// <param name="scannerAssemblies">One or more assemblies to scan for message publishers and consumers.</param>
        /// <returns>The same service collection instance, to enable method chaining.</returns>
        /// <remarks>
        /// <para>
        /// This method performs the following configuration:
        /// <list type="number">
        ///   <item><description>Registers Azure Service Bus clients (client and administration client) with the specified connection string</description></item>
        ///   <item><description>Registers the <see cref="AmqpConnectionProvider"/> as a singleton</description></item>
        ///   <item><description>Scans the provided assemblies for message publishers and consumers and registers them</description></item>
        ///   <item><description>Adds the hosted services needed to manage consumer lifecycle and health checks</description></item>
        ///   <item><description>Configures a health check against a dedicated health check queue</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// Example usage:
        /// <code>
        /// services.AddAmqpClient(
        ///     connectionString: Configuration.GetConnectionString("ServiceBus"),
        ///     clientSettings: options => {
        ///         options.RetryOptions.MaxRetries = 3;
        ///         options.TransportType = ServiceBusTransportType.AmqpWebSockets;
        ///     },
        ///     scannerAssemblies: typeof(Program).Assembly
        /// );
        /// </code>
        /// </para>
        /// </remarks>
        public static IServiceCollection AddAmqpClient(this IServiceCollection services,
                                                       string connectionString,
                                                       Action<ServiceBusClientOptions> clientSettings,
                                                       params Assembly[] scannerAssemblies)
        {
            services.AddAzureClients(azure =>
            {
                azure.AddServiceBusClient(connectionString).ConfigureOptions(clientSettings);
                azure.AddServiceBusAdministrationClient(connectionString);
            });


            // 2. Core connection provider
            services.AddSingleton<AmqpConnectionProvider>();

            // 3. Publishers & Consumers from all supplied assemblies
            services.AddPublishers(scannerAssemblies).AddConsumers(scannerAssemblies);

            // 4. Hosted service for consumer lifecycle
            services.AddHostedService<AmqpConsumerHostedService>().AddHostedService<HealthQueueInitializerHostedService>();

            // 5. Health check against the AMQP connection

            services.AddHealthChecks().AddAzureServiceBusQueue(connectionString, HealthQueueInitializerHostedService.HealthCheckQueue);

            return services;
        }


        /// <summary>
        /// Scans the provided assemblies for message publisher implementations and registers them in the service collection.
        /// </summary>
        /// <param name="services">The service collection to add the publishers to.</param>
        /// <param name="scannerAssemblies">The assemblies to scan for publisher implementations.</param>
        /// <returns>The same service collection instance, to enable method chaining.</returns>
        /// <remarks>
        /// <para>
        /// This method finds all non-abstract classes that inherit from <see cref="MessagePublisher"/> in the provided
        /// assemblies and registers them with a scoped lifetime in the dependency injection container.
        /// </para>
        /// <para>
        /// Publishers are registered with a scoped lifetime to ensure they properly participate in any existing
        /// transaction scope and are not shared between different request contexts.
        /// </para>
        /// </remarks>
        private static IServiceCollection AddPublishers(this IServiceCollection services, params Assembly[] scannerAssemblies)
        {
            foreach (TypeInfo pub in scannerAssemblies.SelectMany(a => a.DefinedTypes).Where(t => t.IsSubclassOf(typeof(MessagePublisher)) && !t.IsAbstract))
                services.AddScoped(pub.AsType());

            return services;
        }

        /// <summary>
        /// Scans the provided assemblies for message consumer implementations and registers them in the service collection.
        /// </summary>
        /// <param name="services">The service collection to add the consumers to.</param>
        /// <param name="scannerAssemblies">The assemblies to scan for consumer implementations.</param>
        /// <returns>The same service collection instance, to enable method chaining.</returns>
        /// <remarks>
        /// <para>
        /// This method finds all classes that are decorated with either <see cref="QueueConsumerAttribute"/> or
        /// <see cref="TopicConsumerAttribute"/> in the provided assemblies and registers them with a transient 
        /// lifetime in the dependency injection container.
        /// </para>
        /// <para>
        /// Consumers are registered with a transient lifetime to ensure a new instance is created for each message
        /// being processed, preventing state from leaking between message processing operations.
        /// </para>
        /// </remarks>
        private static IServiceCollection AddConsumers(this IServiceCollection services, params Assembly[] scannerAssemblies)
        {
            foreach (Type consumer in scannerAssemblies.SelectMany(a => a.GetTypes()).Where(t => t.GetCustomAttributes<QueueConsumerAttribute>().Any() || t.GetCustomAttributes<TopicConsumerAttribute>().Any()))
                services.AddTransient(consumer);

            return services;
        }
    }
}
