namespace Flaminco.AzureBus.AMQP.Extensions
{
    using Flaminco.AzureBus.AMQP.Abstractions;
    using Flaminco.AzureBus.AMQP.Implementation;
    using Flaminco.AzureBus.AMQP.Options;
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
        /// <param name="clientSettings">A delegate to configure the address settings used to connect to the message broker.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddAMQPClient<TScanner>(this IServiceCollection services, Action<AMQPClientSettings> clientSettings)
        {
            services.Configure(clientSettings);

            services.AddSingleton<IAMQPLocator, AMQPLocator>();

            services.AddImplementations<MessagePublisher>(typeof(TScanner));

            services.AddImplementations<MessageConsumer>(typeof(TScanner));

            return services;
        }

        /// <summary>
        /// Registers the AMQP consumer as a hosted service in the dependency injection container.
        /// </summary>
        /// <typeparam name="TConsumer">The type of the consumer that processes the AMQP messages. Must inherit from <see cref="MessageConsumer"/>.</typeparam>
        /// <typeparam name="TMessage">The type of the message that the consumer will handle. Must be a non-nullable type.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the hosted service to.</param>
        /// <returns>The modified <see cref="IServiceCollection"/> for chaining additional service registrations.</returns>
        public static IServiceCollection AddAMQPService<TConsumer, TMessage>(this IServiceCollection services) where TConsumer : MessageConsumer where TMessage : notnull
        {
            return services.AddHostedService<AMQPBackgroundService<TConsumer, TMessage>>();
        }


        private static void AddImplementations<TAbstract>(this IServiceCollection services, Type scannerType)
        {
            foreach (var type in scannerType.Assembly.DefinedTypes.Where(type => type.IsSubclassOf(typeof(TAbstract)) && !type.IsAbstract))
            {
                services.AddScoped(typeof(TAbstract), type);
            }
        }

    }

}
