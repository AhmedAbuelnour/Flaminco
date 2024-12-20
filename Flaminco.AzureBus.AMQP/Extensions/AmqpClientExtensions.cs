﻿using Azure.Messaging.ServiceBus.Administration;
using Flaminco.AzureBus.AMQP.Abstractions;
using Flaminco.AzureBus.AMQP.Attributes;
using Flaminco.AzureBus.AMQP.Models;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Reflection;

namespace Flaminco.AzureBus.AMQP.Extensions;

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

        services.AddSyncPublishers<TScanner>();

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

        var consumerTypes = typeof(TScanner).Assembly
            .GetTypes()
            .Where(t => t.GetCustomAttributes<QueueConsumerAttribute>().Any() ||
                        t.GetCustomAttributes<TopicConsumerAttribute>().Any())
            .ToList();


        services.AddMassTransit(bus =>
        {
            // Register your consumer with MassTransit
            foreach (var consumer in consumerTypes)
                // Register the consumer dynamically
                bus.AddConsumer(consumer);


            foreach (Type syncQueueConsumerType in typeof(TScanner).Assembly.GetTypes().Where(t => t.GetCustomAttributes<SyncQueueConsumerAttribute>().Any()).ToList())
            {
                if (syncQueueConsumerType.GetCustomAttribute<SyncQueueConsumerAttribute>() is SyncQueueConsumerAttribute syncQueueConsumerAttribute)
                {
                    bus.AddRequestClient(syncQueueConsumerAttribute.MessageType, new Uri($"queue:{syncQueueConsumerAttribute.Queue}"), clientSettings.SyncQueuePublisherTimeOut ?? RequestTimeout.Default);
                }
            }

            if (clientSettings.HealthCheckOptions is null)
            {
                bus.ConfigureHealthCheckOptions(options =>
                {
                    options.Name = "MassTransit-AzureBus";
                    options.MinimalFailureStatus = HealthStatus.Unhealthy;
                    options.Tags.Add("health");
                });
            }
            else
            {
                bus.ConfigureHealthCheckOptions(option =>
                {
                    option.MinimalFailureStatus = clientSettings.HealthCheckOptions?.MinimalFailureStatus ?? HealthStatus.Unhealthy;
                    option.Name = clientSettings.HealthCheckOptions?.Name ?? "MassTransit-AzureBus";

                    foreach (string tag in new List<string>(clientSettings.HealthCheckOptions?.Tags ?? ["health"]))
                    {
                        option.Tags.Add(tag);
                    }
                });
            }

            // Configure RabbitMQ for MassTransit
            bus.UsingAzureServiceBus((context, cfg) =>
            {
                cfg.Host(clientSettings.Host);

                if (clientSettings.SkipMessageTypeMatching == true)
                    cfg.UseRawJsonSerializer(RawSerializerOptions.All, true);

                if (clientSettings.RetryCount.HasValue && clientSettings.RetryInterval.HasValue)
                    cfg.UseMessageRetry(x =>
                        x.Interval(clientSettings.RetryCount.Value, clientSettings.RetryInterval.Value));

                foreach (var consumer in consumerTypes)
                {
                    if (consumer.GetCustomAttribute<QueueConsumerAttribute>() is QueueConsumerAttribute queueConsumer)
                    {
                        cfg.ReceiveEndpoint(queueConsumer.Queue, endpointConfig =>
                        {
                            // Dynamically configure the consumer for the queue
                            endpointConfig.ConfigureConsumer(context, consumer);
                        });

                        continue;
                    }

                    if (consumer.GetCustomAttribute<TopicConsumerAttribute>() is TopicConsumerAttribute topicConsumer)
                        cfg.SubscriptionEndpoint(topicConsumer.Subscription, topicConsumer.Topic, e =>
                        {
                            if (topicConsumer.RuleFilterType is not null &&
                                Activator.CreateInstance(topicConsumer.RuleFilterType) is IRuleFilterProvider
                                    ruleFilterProvider)
                                if (ruleFilterProvider.GetRuleFilter() is RuleFilter ruleFilter)
                                    e.Filter = ruleFilter;

                            // Dynamically configure the topic consumer
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
        foreach (var type in typeof(TScanner).Assembly.DefinedTypes.Where(type => !type.IsAbstract &&
                     type.BaseType != null &&
                     type.BaseType.IsGenericType &&
                     type.BaseType.GetGenericTypeDefinition() == typeof(MessagePublisher<>)))
            services.AddScoped(type);
    }

    /// <summary>
    ///     Adds flows registrations to the service collection for the specified scanner type.
    /// </summary>
    /// <typeparam name="TScanner">A type used to scan for flow implementations.</typeparam>
    /// <param name="services">The service collection to which the flows will be added.</param>
    private static void AddSyncPublishers<TScanner>(this IServiceCollection services)
    {
        foreach (var type in typeof(TScanner).Assembly.DefinedTypes.Where(type => !type.IsAbstract &&
                     type.BaseType != null &&
                     type.BaseType.IsGenericType &&
                     type.BaseType.GetGenericTypeDefinition() == typeof(SyncMessagePublisher<>)))
            services.AddScoped(type);
    }

}