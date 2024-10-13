namespace Flaminco.RabbitMQ.AMQP.Extensions
{
    using Flaminco.RabbitMQ.AMQP.Abstractions;
    using Flaminco.RabbitMQ.AMQP.Models;
    using MassTransit;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Provides extension methods for configuring AMQP clients and registering publishers and consumers in the service collection.
    /// </summary>
    public static class AMQPClientExtensions
    {
        /// <summary>
        /// Configures the AMQP client by registering the AMQP locator, publishers, and consumers in the service collection.
        /// </summary>
        /// <typeparam name="TScanner">A type from the assembly to scan for message publisher implementations or consumer implementations.</typeparam>
        /// <param name="services">The service collection to which the AMQP client, publishers, and consumers will be added.</param>
        /// <param name="clientSettings">The settings used to configure the AMQP client.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddAMQPClient<TScanner>(this IServiceCollection services, Action<AMQPClientSettings> clientSettings)
        {
            services.AddPublishers<TScanner>();

            services.AddConsumers<TScanner>(clientSettings);

            return services;
        }
        /// <summary>
        /// Adds consumer registrations to the service collection for the specified scanner type.
        /// </summary>
        /// <typeparam name="TScanner">A type used to scan for consumer implementations.</typeparam>
        /// <param name="services">The service collection to which the consumers will be added.</param>
        /// <param name="anqoClientSettings">The AMQP client settings for RabbitMQ configuration.</param>
        /// <returns>The updated service collection.</returns>
        private static IServiceCollection AddConsumers<TScanner>(this IServiceCollection services, Action<AMQPClientSettings> anqoClientSettings)
        {
            AMQPClientSettings clientSettings = new();

            anqoClientSettings(clientSettings);

            IEnumerable<QueueConfiguration> queueConfigurations = GetQueueConfiguration(typeof(TScanner)).ToList();

            services.AddMassTransit(x =>
            {
                // Register your consumer with MassTransit

                foreach (var config in queueConfigurations)
                {
                    // Register the consumer dynamically
                    x.AddConsumer(config.ConsumerType);
                }

                // Configure RabbitMQ for MassTransit
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(new Uri(clientSettings.Host), h =>
                    {
                        h.Username(clientSettings.Username);
                        h.Password(clientSettings.Password);
                    });

                    // Dynamically add queues and consumers
                    foreach (var config in queueConfigurations)
                    {
                        cfg.ReceiveEndpoint(config.QueueName, e =>
                        {
                            // Dynamically configure the consumer for the queue
                            e.ConfigureConsumer(context, config.ConsumerType);
                        });
                    }
                });
            });

            return services;
        }

        /// <summary>
        /// Adds publisher registrations to the service collection for the specified scanner type.
        /// </summary>
        /// <typeparam name="TScanner">A type used to scan for publisher implementations.</typeparam>
        /// <param name="services">The service collection to which the publishers will be added.</param>
        private static void AddPublishers<TScanner>(this IServiceCollection services)
        {
            foreach (var type in typeof(TScanner).Assembly.DefinedTypes.Where(type => type.IsSubclassOf(typeof(MessagePublisher)) && !type.IsAbstract))
            {
                services.AddScoped(type);
            }
        }

        /// <summary>
        /// Gets the queue configuration by scanning for classes that inherit from <see cref="MessageConsumer{TMessage}"/>.
        /// </summary>
        /// <param name="scannerType">The type used to scan for message consumer implementations.</param>
        /// <returns>An enumerable of <see cref="QueueConfiguration"/> that contains queue names and consumer types.</returns>
        private static IEnumerable<QueueConfiguration> GetQueueConfiguration(Type scannerType)
        {
            // Iterate over all types in the assembly of TScanner
            foreach (var type in scannerType.Assembly.DefinedTypes.Where(type => !type.IsAbstract && !type.IsInterface))
            {
                // Check if the type inherits from a generic version of MessageConsumer<TMessage>
                var baseType = type.BaseType;

                if (baseType != null && baseType.IsGenericType &&
                    baseType.GetGenericTypeDefinition() == typeof(MessageConsumer<>))
                {
                    // Get the generic argument (TMessage)
                    Type messageType = baseType.GetGenericArguments()[0];

                    // Check if the generic argument implements IMessage
                    if (typeof(IMessage).IsAssignableFrom(messageType))
                    {
                        // Use reflection to get the 'Queue' property without instantiating the class
                        var queueProperty = type.GetProperty("Queue", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                        if (queueProperty != null)
                        {
                            // Since we're not instantiating, we'll treat Queue as a static property here for simplicity
                            string? queueName = queueProperty.GetValue(Activator.CreateInstance(type))?.ToString();

                            if (!string.IsNullOrWhiteSpace(queueName))
                            {
                                yield return new QueueConfiguration
                                {
                                    QueueName = queueName,
                                    ConsumerType = type
                                };
                            }
                        }
                    }
                }
            }
        }
    }
}
