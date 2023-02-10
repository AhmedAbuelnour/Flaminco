
namespace Flaminco.ServiceBus.Extensions
{
    using Flaminco.ServiceBus.Abstractions;
    using Flaminco.ServiceBus.Implementation;
    using Flaminco.ServiceBus.Options;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class BusClientExtensions
    {
        public static IServiceCollection AddBusClient<TAssemblyScanner>(this IServiceCollection services, IConfiguration configuration)
        {
            IEnumerable<TypeInfo> publisherTypes = from type in typeof(TAssemblyScanner).Assembly.DefinedTypes
                                                   where !type.IsAbstract && typeof(MessagePublisher).IsAssignableFrom(type)
                                                   select type.GetTypeInfo();

            foreach (Type? type in publisherTypes)
            {
                services.AddSingleton(typeof(MessagePublisher), type);
            }

            IEnumerable<TypeInfo> queueConsumerTypes = from type in typeof(TAssemblyScanner).Assembly.DefinedTypes
                                                       where !type.IsAbstract && typeof(MessageQueueConsumer).IsAssignableFrom(type)
                                                       select type.GetTypeInfo();

            foreach (Type? type in queueConsumerTypes)
            {
                services.AddSingleton(typeof(MessageQueueConsumer), type);
            }


            IEnumerable<TypeInfo> topicConsumerTypes = from type in typeof(TAssemblyScanner).Assembly.DefinedTypes
                                                       where !type.IsAbstract && typeof(MessageTopicConsumer).IsAssignableFrom(type)
                                                       select type.GetTypeInfo();

            foreach (Type? type in topicConsumerTypes)
            {
                services.AddSingleton(typeof(MessageTopicConsumer), type);
            }

            services.Configure<ServiceBusSettings>(configuration.GetSection(nameof(ServiceBusSettings)));


            services.AddSingleton<IServiceBusLocator, ServiceBusLocator>();

            return services;

        }
    }
}
