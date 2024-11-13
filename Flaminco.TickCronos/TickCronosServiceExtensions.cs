using Microsoft.Extensions.DependencyInjection;

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
        public static IServiceCollection AddTickCronosJob<T>(this IServiceCollection services, Action<ITickCronosConfig<T>> options) where T : TickCronosJob
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options), "Please provide Schedule Configurations.");
            }

            ITickCronosConfig<T> config = new TickCronosConfig<T>();

            options.Invoke(config);

            services.AddSingleton(config);

            services.AddHostedService<T>();

            return services;
        }
    }
}