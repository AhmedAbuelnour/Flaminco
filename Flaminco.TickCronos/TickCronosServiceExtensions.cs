﻿using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.TickCronos
{
    /// <summary>
    /// Provides extension methods for adding scheduled services.
    /// </summary>
    public static class TickCronosServiceExtensions
    {
        /// <summary>
        /// Adds a cron job service to the service collection.
        /// </summary>
        /// <typeparam name="T">The type of the cron job service to add.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="options">The configuration options for the cron job service.</param>
        /// <returns>The updated service collection.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the options or cron expression is null or empty.</exception>
        public static IServiceCollection AddTickCronosJob<T>(this IServiceCollection services, Action<TickCronosConfig<T>> options) where T : TickCronosJobService
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options), "Please provide Schedule Configurations.");
            }

            TickCronosConfig<T> config = new();

            options.Invoke(config);

            if (string.IsNullOrWhiteSpace(config.CronExpression))
            {
                throw new ArgumentNullException(nameof(options), "Empty Cron Expression is not allowed.");
            }

            services.AddSingleton<ITickCronosConfig<T>>(config);

            services.AddHostedService<T>();

            return services;
        }
    }
}