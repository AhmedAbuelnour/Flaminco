using Flaminco.AzureBus.AMQP.Attributes;
using Flaminco.RabbitMQ.AMQP.Abstractions;
using Flaminco.RabbitMQ.AMQP.Attributes;
using Flaminco.RabbitMQ.AMQP.Models;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Flaminco.RabbitMQ.AMQP.Extensions;

/// <summary>
///     Provides extension methods for configuring AMQP clients and registering publishers and consumers in the service
///     collection.
/// </summary>
public static class AMQPClientExtensions
{
    /// <summary>
    ///     Configures the AMQP client by registering the AMQP locator, publishers, and consumers in the service collection.
    /// </summary>
    /// <typeparam name="TScanner">
    ///     A type from the assembly to scan for message publisher implementations or consumer
    ///     implementations.
    /// </typeparam>
    /// <param name="services">The service collection to which the AMQP client, publishers, and consumers will be added.</param>
    /// <param name="clientSettings">The settings used to configure the AMQP client.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddAMQPClient<TScanner>(this IServiceCollection services,
        Action<AMQPClientSettings> clientSettings)
    {
        services.AddPublishers<TScanner>();

        services.AddMessageFlows<TScanner>();

        services.AddConsumers<TScanner>(clientSettings);

        return services;
    }

    /// <summary>
    ///     Adds consumer registrations to the service collection for the specified scanner type.
    /// </summary>
    /// <typeparam name="TScanner">A type used to scan for consumer implementations.</typeparam>
    /// <param name="services">The service collection to which the consumers will be added.</param>
    /// <param name="amqpClientSettings">The AMQP client settings for RabbitMQ configuration.</param>
    /// <returns>The updated service collection.</returns>
    private static IServiceCollection AddConsumers<TScanner>(this IServiceCollection services,
        Action<AMQPClientSettings> amqpClientSettings)
    {
        AMQPClientSettings clientSettings = new();

        amqpClientSettings(clientSettings);

        var consumerTypes = typeof(TScanner).Assembly.GetTypes()
            .Where(t => t.GetCustomAttributes<QueueConsumerAttribute>().Any()).ToList();

        services.AddMassTransit(x =>
        {
            // Register your consumer with MassTransit
            foreach (var consumer in consumerTypes)
                // Register the consumer dynamically
                x.AddConsumer(consumer);

            foreach (Type messageFlowType in typeof(TScanner).Assembly.GetTypes().Where(t => t.GetCustomAttributes<MessageFlowAttribute>().Any()).ToList())
            {
                if (messageFlowType.GetCustomAttribute<MessageFlowAttribute>() is MessageFlowAttribute messageFlow)
                {
                    x.AddRequestClient(messageFlow.MessageType, new Uri($"queue:{messageFlow.Queue}"), clientSettings.MessageFlowTimeOut ?? RequestTimeout.Default);
                }
            }

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(clientSettings.Host, h =>
                {
                    h.Username(clientSettings.Username);
                    h.Password(clientSettings.Password);
                });

                if (clientSettings.RetryCount.HasValue && clientSettings.RetryInterval.HasValue)
                    cfg.UseMessageRetry(x =>
                        x.Interval(clientSettings.RetryCount.Value, clientSettings.RetryInterval.Value));

                foreach (var consumer in consumerTypes)
                    if (consumer.GetCustomAttribute<QueueConsumerAttribute>() is QueueConsumerAttribute queueConsumer)
                    {
                        cfg.ReceiveEndpoint(queueConsumer.Queue, e =>
                        {
                            // Dynamically configure the consumer for the queue
                            e.ConfigureConsumer(context, consumer);
                        });
                    }
            });
        });

        return services;
    }

    /// <summary>
    ///     Adds publisher registrations to the service collection for the specified scanner type.
    /// </summary>
    /// <typeparam name="TScanner">A type used to scan for publisher implementations.</typeparam>
    /// <param name="services">The service collection to which the publishers will be added.</param>
    private static void AddPublishers<TScanner>(this IServiceCollection services)
    {
        foreach (var type in typeof(TScanner).Assembly.DefinedTypes.Where(type =>
                     type.IsSubclassOf(typeof(MessagePublisher)) && !type.IsAbstract)) services.AddScoped(type);
    }

    /// <summary>
    ///     Adds flows registrations to the service collection for the specified scanner type.
    /// </summary>
    /// <typeparam name="TScanner">A type used to scan for flow implementations.</typeparam>
    /// <param name="services">The service collection to which the flows will be added.</param>
    private static void AddMessageFlows<TScanner>(this IServiceCollection services)
    {
        foreach (var type in typeof(TScanner).Assembly.DefinedTypes.Where(type => !type.IsAbstract &&
                     type.BaseType != null &&
                     type.BaseType.IsGenericType &&
                     type.BaseType.GetGenericTypeDefinition() == typeof(MessageFlow<>)))
            services.AddScoped(type);
    }
}