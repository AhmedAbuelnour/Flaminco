// Implementation of cron schedule configuration
namespace Flaminco.TickCronos
{

    /// <summary>
    /// Represents the configuration interface for a scheduled cron job.
    /// </summary>
    /// <typeparam name="T">The type of the cron job service.</typeparam>
    public interface ITickCronosConfig<T> where T : TickCronosJobService
    {
        /// <summary>
        /// Gets or sets the cron expression used for scheduling.
        /// </summary>
        string CronExpression { get; set; }

        /// <summary>
        /// Gets or sets the time provider used for the cron job service.
        /// </summary>
        TimeProvider TimeProvider { get; set; }
    }

    /// <inheritdoc/>
    public class TickCronosConfig<T> : ITickCronosConfig<T> where T : TickCronosJobService
    {
        /// <inheritdoc/>
        public string CronExpression { get; set; } = default!;
        /// <inheritdoc/>
        public TimeProvider TimeProvider { get; set; } = TimeProvider.System;
    }
}