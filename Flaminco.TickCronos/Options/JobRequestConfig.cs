using Flaminco.TickCronos.Abstractions;

namespace Flaminco.TickCronos.Options
{

    /// <summary>
    /// Represents the configuration interface for a scheduled cron job.
    /// </summary>
    /// <typeparam name="T">The type of the cron job service.</typeparam>
    public interface IJobRequestConfig<T> where T : JobRequest
    {
        /// <summary>
        /// Gets or sets the cron expression used for scheduling.
        /// </summary>
        string? CronExpression { get; set; }

        /// <summary>
        /// Gets or sets the time provider used for the cron job service.
        /// </summary>
        TimeProvider TimeProvider { get; set; }
    }

    /// <inheritdoc/>
    internal class JobRequestConfig<T> : IJobRequestConfig<T> where T : JobRequest
    {
        /// <inheritdoc/>
        public string? CronExpression { get; set; }
        /// <inheritdoc/>
        public TimeProvider TimeProvider { get; set; } = TimeProvider.System;
    }

}