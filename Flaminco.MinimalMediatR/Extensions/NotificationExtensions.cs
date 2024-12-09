using Flaminco.MinimalMediatR.Abstractions;
using Flaminco.MinimalMediatR.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Flaminco.MinimalMediatR.Extensions
{
    public static class NotificationExtensions
    {
        public static IServiceCollection AddChannelPublishers(this IServiceCollection services)
        {
            services.AddSingleton<INotificationErrorHandler, DefaultErrorHandler>();

            services.AddSingleton<InMemoryMessageQueue>();

            services.AddSingleton<IChannelPublisher, DefaultChannelPublisherImp>();

            services.AddHostedService<DefaultNotificationProcessor>();

            return services;
        }

        public static IServiceCollection TryAddNotificationErrorHandler<T>(this IServiceCollection services) where T : class, INotificationErrorHandler
        {
            services.TryAddSingleton<INotificationErrorHandler, T>();

            return services;
        }
    }
}
