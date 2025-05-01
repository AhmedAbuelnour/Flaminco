using Flaminco.TickCronos.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.TickCronos.Extensions
{

    /// <summary>
    /// Provides extension methods for registering job queue services in the dependency injection container.
    /// </summary>
    public static class QueueExtensions
    {
        /// <summary>
        /// Registers the job queue and its associated background processor in the service collection.
        /// </summary>
        /// <param name="services">The service collection to register the job queue services into.</param>
        /// <returns>A <see cref="JobQueueBuilder"/> instance for further configuration.</returns>
        public static JobQueueBuilder AddJobQueue(this IServiceCollection services)
        {
            return new JobQueueBuilder(services);
        }
    }
}
