using Flaminco.MinimalEndpoints.Abstractions;
using Flaminco.MinimalEndpoints.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.MinimalEndpoints.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring the event bus functionality.
    /// </summary>
    public static class EventBusExtensions
    {
        /// <summary>
        /// Registers the event bus and its background processor in the service collection.
        /// </summary>
        /// <param name="services">The service collection to register the event bus services into.</param>
        /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
        public static IServiceCollection AddEventBus<TScanner>(this IServiceCollection services)
        {
            services.AddSingleton<EventBus>();
            services.AddHostedService<EventProcessor>();

            foreach (Type handlerType in typeof(TScanner).Assembly.GetTypes()
                                                                  .Where(t => t.IsClass
                                                                     && !t.IsAbstract
                                                                     && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>))))
            {
                foreach (Type handlerInterface in handlerType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)))
                {
                    services.AddTransient(handlerInterface, handlerType);
                }
            }

            return services;
        }
    }
}
