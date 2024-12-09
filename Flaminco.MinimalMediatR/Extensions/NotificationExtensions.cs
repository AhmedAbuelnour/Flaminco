﻿using Flaminco.MinimalMediatR.Abstractions;
using Flaminco.MinimalMediatR.Implementations;
using Microsoft.Extensions.DependencyInjection;

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
    }
}
