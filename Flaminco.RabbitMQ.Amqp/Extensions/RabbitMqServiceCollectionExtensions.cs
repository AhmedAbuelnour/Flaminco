using Flaminco.RabbitMQ.AMQP.Abstractions;
using Flaminco.RabbitMQ.AMQP.Attributes;
using Flaminco.RabbitMQ.AMQP.Configuration;
using Flaminco.RabbitMQ.AMQP.HealthChecks;
using Flaminco.RabbitMQ.AMQP.HostedServices;
using Flaminco.RabbitMQ.AMQP.Serialization;
using Flaminco.RabbitMQ.AMQP.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Flaminco.RabbitMQ.AMQP.Extensions;

/// <summary>
/// Extension methods for configuring RabbitMQ services in the dependency injection container.
/// </summary>
public static class RabbitMqServiceCollectionExtensions
{
    /// <summary>
    /// Adds RabbitMQ services to the service collection using configuration from appsettings.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="configureTopology">Action to configure topology using fluent API.</param>
    /// <param name="assemblies">Assemblies to scan for consumers. If not provided, scans entry assembly.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<TopologyConfiguration> configureTopology,
        params Assembly[] assemblies)
    {
        // Bind RabbitMQ options from configuration
        var optionsSection = configuration.GetSection(RabbitMqOptions.SectionName);
        services.Configure<RabbitMqOptions>(options => optionsSection.Bind(options));

        return services.AddRabbitMqCore(configureTopology, assemblies);
    }

    /// <summary>
    /// Adds RabbitMQ services to the service collection using fluent configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure RabbitMQ options.</param>
    /// <param name="configureTopology">Action to configure topology using fluent API.</param>
    /// <param name="assemblies">Assemblies to scan for consumers.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRabbitMq(
        this IServiceCollection services,
        Action<RabbitMqOptions> configureOptions,
        Action<TopologyConfiguration> configureTopology,
        params Assembly[] assemblies)
    {
        services.Configure(configureOptions);

        return services.AddRabbitMqCore(configureTopology, assemblies);
    }

    private static IServiceCollection AddRabbitMqCore(
        this IServiceCollection services,
        Action<TopologyConfiguration> configureTopology,
        params Assembly[] assemblies)
    {
        // Configure fluent topology
        var topology = new TopologyConfiguration();
        configureTopology(topology);
        services.Configure<TopologyConfiguration>(t =>
        {
            t.Exchanges.AddRange(topology.Exchanges);
            t.Queues.AddRange(topology.Queues);
            t.Bindings.AddRange(topology.Bindings);
        });

        // Core services
        services.AddSingleton<IRabbitMqConnectionManager, RabbitMqConnectionManager>();
        services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();
        services.AddSingleton<TopologyInitializer>();
        services.AddScoped<IMessagePublisher, RabbitMqPublisher>();

        // Hosted service for consumers
        services.AddSingleton<IHostedService, RabbitMqConsumerHostedService>();

        // Register consumers
        var assembliesToScan = assemblies.Length > 0
            ? assemblies
            : [Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()];

        services.RegisterConsumers(assembliesToScan);

        // Health check
        services.AddHealthChecks()
            .AddCheck<RabbitMqHealthCheck>("rabbitmq", tags: ["rabbitmq", "messaging"]);

        return services;
    }

    /// <summary>
    /// Registers a custom message serializer.
    /// </summary>
    /// <typeparam name="TSerializer">The serializer type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection UseMessageSerializer<TSerializer>(this IServiceCollection services)
        where TSerializer : class, IMessageSerializer
    {
        // Remove existing serializer registration
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IMessageSerializer));
        if (descriptor is not null)
        {
            services.Remove(descriptor);
        }

        services.AddSingleton<IMessageSerializer, TSerializer>();
        return services;
    }

    private static void RegisterConsumers(this IServiceCollection services, Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsAbstract &&
                        !type.IsInterface &&
                        IsMessageConsumer(type) &&
                        type.GetCustomAttribute<QueueAttribute>() is not null)
                    {
                        services.AddScoped(type);
                    }
                }
            }
            catch (ReflectionTypeLoadException)
            {
                // Ignore assemblies that can't be loaded
            }
        }
    }

    private static bool IsMessageConsumer(Type type)
    {
        var baseType = type.BaseType;
        while (baseType is not null)
        {
            if (baseType.IsGenericType &&
                baseType.GetGenericTypeDefinition() == typeof(MessageConsumer<>))
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }
}
