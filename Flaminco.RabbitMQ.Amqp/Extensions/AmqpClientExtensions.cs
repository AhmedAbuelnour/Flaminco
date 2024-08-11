using Flaminco.RabbitMQ.AMQP.Abstractions;
using Flaminco.RabbitMQ.AMQP.Implementation;
using Flaminco.RabbitMQ.AMQP.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.RabbitMQ.AMQP.Extensions
{
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
        /// <param name="addressSettings">A delegate to configure the address settings used to connect to the message broker.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddAMQPClient<TScanner>(this IServiceCollection services, Action<AddressSettings> addressSettings)
        {
            services.Configure(addressSettings);

            services.AddSingleton<IAMQPLocator, AMQPLocator>();

            services.AddImplementations<MessagePublisher>(typeof(TScanner));

            services.AddImplementations<MessageConsumer>(typeof(TScanner));

            return services;
        }

        private static void AddImplementations<TAbstract>(this IServiceCollection services, Type scannerType)
        {
            foreach (var type in scannerType.Assembly.DefinedTypes.Where(type => type.IsSubclassOf(typeof(TAbstract)) && !type.IsAbstract))
            {
                services.AddSingleton(typeof(TAbstract), type);
            }
        }
    }

}
