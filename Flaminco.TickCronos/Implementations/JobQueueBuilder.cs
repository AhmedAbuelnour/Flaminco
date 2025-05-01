using Flaminco.TickCronos.Abstractions;
using Flaminco.TickCronos.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.TickCronos.Implementations
{
    /// <summary>
    /// Builder class for configuring and adding jobs to the job queue.
    /// </summary>
    public sealed class JobQueueBuilder(IServiceCollection services)
    {
        /// <summary>
        /// Adds a job to the job queue with the specified configuration options.
        /// </summary>
        /// <typeparam name="T">The type of the job request.</typeparam>
        /// <param name="options">The configuration options for the job.</param>
        /// <returns>The current instance of <see cref="JobQueueBuilder"/> for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the options parameter is null.</exception>
        public JobQueueBuilder AddJob<T>(Action<IJobRequestConfig<T>> options) where T : JobRequest
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options), "Please provide Schedule Configurations.");
            }

            IJobRequestConfig<T> config = new JobRequestConfig<T>();

            options.Invoke(config);

            // Register the configuration and job.
            services.AddSingleton(config);

            services.AddHostedService<T>();

            return this; // return builder for chaining
        }
    }
}
