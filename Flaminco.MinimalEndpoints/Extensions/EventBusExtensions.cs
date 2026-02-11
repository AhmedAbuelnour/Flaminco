using Flaminco.MinimalEndpoints.Abstractions;
using Flaminco.MinimalEndpoints.Implementations;
using Flaminco.MinimalEndpoints.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace Flaminco.MinimalEndpoints.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring the event bus functionality.
    /// </summary>
    public static class EventBusExtensions
    {
        public static IServiceCollection AddEventBus<TScanner>(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddSingleton<EventBus>();
            services.TryAddSingleton<IEventBus>(sp => sp.GetRequiredService<EventBus>());
            services.AddHostedService<EventProcessor>();

            var handlerTypes = typeof(TScanner).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => t.GetInterfaces().Any(IsEventHandlerInterface))
                .ToArray();

            foreach (var handlerType in handlerTypes)
            {
                var handlerInterfaces = handlerType.GetInterfaces()
                    .Where(IsEventHandlerInterface);

                foreach (var handlerInterface in handlerInterfaces)
                {
                    services.TryAddEnumerable(ServiceDescriptor.Transient(handlerInterface, handlerType));
                }
            }

            return services;
        }

        public static IServiceCollection AddEventBus(this IServiceCollection services, params Assembly[] assemblies)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddSingleton<EventBus>();
            services.TryAddSingleton<IEventBus>(sp => sp.GetRequiredService<EventBus>());
            services.AddHostedService<EventProcessor>();

            foreach (var assembly in assemblies)
            {
                var handlerTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract)
                    .Where(t => t.GetInterfaces().Any(IsEventHandlerInterface))
                    .ToArray();

                foreach (var handlerType in handlerTypes)
                {
                    foreach (var handlerInterface in handlerType.GetInterfaces().Where(IsEventHandlerInterface))
                    {
                        services.TryAddEnumerable(ServiceDescriptor.Transient(handlerInterface, handlerType));
                    }
                }
            }

            return services;
        }

        /// <summary>
        /// Registers a Redis-backed EventBus implementation and handlers scanner.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configureOptions">Redis EventBus options configuration.</param>
        /// <param name="assemblies">Assemblies to scan for IDomainEventHandler implementations.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddRedisEventBus(
            this IServiceCollection services,
            Action<RedisEventBusOptions> configureOptions,
            params Assembly[] assemblies)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configureOptions);

            var options = new RedisEventBusOptions();
            configureOptions(options);

            services.AddSingleton(options);
            services.TryAddSingleton<IEventBus, RedisEventBus>();
            services.AddHostedService<EventProcessor>();

            foreach (var assembly in assemblies)
            {
                var handlerTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract)
                    .Where(t => t.GetInterfaces().Any(IsEventHandlerInterface))
                    .ToArray();

                foreach (var handlerType in handlerTypes)
                {
                    foreach (var handlerInterface in handlerType.GetInterfaces().Where(IsEventHandlerInterface))
                    {
                        services.TryAddEnumerable(ServiceDescriptor.Transient(handlerInterface, handlerType));
                    }
                }
            }

            return services;
        }

        private static bool IsEventHandlerInterface(Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>);
    }
}
