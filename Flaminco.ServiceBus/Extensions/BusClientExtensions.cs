
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
        public static IServiceCollection AddBusLocator(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ServiceBusSettings>(configuration.GetSection(nameof(ServiceBusSettings)));

            services.AddSingleton<IServiceBusLocator, ServiceBusLocator>();

            return services;

        }

        public static IServiceCollection AddPublishers<TPublisherScanner>(this IServiceCollection services)
        {
            IEnumerable<TypeInfo> publisherTypes = from type in typeof(TPublisherScanner).Assembly.DefinedTypes
                                                   where !type.IsAbstract && typeof(MessagePublisher).IsAssignableFrom(type)
                                                   select type.GetTypeInfo();

            foreach (Type? type in publisherTypes)
            {
                services.AddSingleton(typeof(MessagePublisher), type);
            }

            return services;
        }

        public static IServiceCollection AddConsumers<TConsumerScanner>(this IServiceCollection services)
        {

            IEnumerable<TypeInfo> queueConsumerTypes = from type in typeof(TConsumerScanner).Assembly.DefinedTypes
                                                       where !type.IsAbstract && typeof(MessageQueueConsumer).IsAssignableFrom(type)
                                                       select type.GetTypeInfo();

            foreach (Type? type in queueConsumerTypes)
            {
                services.AddSingleton(typeof(MessageQueueConsumer), type);
            }


            IEnumerable<TypeInfo> topicConsumerTypes = from type in typeof(TConsumerScanner).Assembly.DefinedTypes
                                                       where !type.IsAbstract && typeof(MessageTopicConsumer).IsAssignableFrom(type)
                                                       select type.GetTypeInfo();

            foreach (Type? type in topicConsumerTypes)
            {
                services.AddSingleton(typeof(MessageTopicConsumer), type);
            }

            return services;
        }
    }
}
