using Cronos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Flaminco.TickCronos;

/// <summary>
/// Represents an abstract base class for implementing a cron-based background job service.
/// </summary>
/// <param name="cronExpression">The cron expression for scheduling the job.</param>
/// <param name="timeProvider">The time provider used to obtain current time and time zones.</param>
/// <param name="logger">The logger used for logging messages.</param>
public abstract class TickCronosJob(string? cronExpression,
                                    TimeProvider timeProvider,
                                    ILogger logger) : IHostedService, IDisposable
{

    private Task? _executingTask;
    private CancellationTokenSource? _stoppingCts;
    private CronExpression? _expression;
    /// <summary>
    /// Gets the name of the cron job.
    /// </summary>
    public abstract string CronJobName { get; }

    /// <summary>
    /// Starts the cron job service asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to signal the start process.</param>
    /// <returns>A task representing the start process.</returns>
    public virtual Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("{jobName}: started with expression [{expression}].", CronJobName, cronExpression);

        _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _executingTask = ScheduleJob(_stoppingCts.Token);

        return _executingTask.IsCompleted ? _executingTask : Task.CompletedTask;
    }

    /// <summary>
    /// Determines the next scheduled occurrence for the job based on the cron expression.
    /// </summary>
    /// <returns>
    /// A <see cref="DateTimeOffset"/> representing the next occurrence of the job.
    /// </returns>
    public virtual DateTimeOffset? ConfigureNextOccurrence()
    {
        if (!string.IsNullOrWhiteSpace(cronExpression))
        {
            _expression ??= CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);

            return _expression.GetNextOccurrence(timeProvider.GetUtcNow(), timeProvider.LocalTimeZone);
        }

        return null;
    }

    /// <summary>
    /// Schedules the job according to the cron expression.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to signal the scheduling process.</param>
    /// <returns>A task that completes when the job is scheduled.</returns>
    private async Task ScheduleJob(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                DateTimeOffset nowTime = timeProvider.GetUtcNow();

                DateTimeOffset? next = ConfigureNextOccurrence();

                if (!next.HasValue)
                {
                    logger.LogInformation("{jobName}: No upcoming schedules, job will not continue", CronJobName);

                    break;
                }

                TimeSpan delay = RoundToNearestSecond(next.Value - nowTime);

                if (delay.TotalMilliseconds <= 0)
                {
                    logger.LogInformation("{jobName}: Next occurrence is in the past, skipping...", CronJobName);

                    continue;
                }

                using PeriodicTimer periodicTimer = new(delay);

                await periodicTimer.WaitForNextTickAsync(cancellationToken);  // Wait until the next schedule

                if (!cancellationToken.IsCancellationRequested)
                {
                    await ExecuteAsync(cancellationToken);

                    logger.LogInformation("{jobName}: Job executed at {time}", CronJobName, timeProvider.GetLocalNow());
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("{jobName}: job received cancellation signal, stopping...", CronJobName);
            }
            catch (Exception e)
            {
                logger.LogError(e, "{jobName}: an error occurred while scheduling the job", CronJobName);
            }
        }
    }

    /// <summary>
    /// Executes the job logic asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to signal the execution process.</param>
    /// <returns>A task representing the execution process.</returns>
    public abstract Task ExecuteAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Stops the cron job service asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to signal the stop process.</param>
    /// <returns>A task representing the stop process.</returns>
    public virtual async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("{jobName}: stopping...", CronJobName);

        if (_stoppingCts is not null)
        {
            await _stoppingCts.CancelAsync();
        }

        logger.LogInformation("{jobName}: stopped.", CronJobName);
    }

    /// <summary>
    /// Releases resources used by the cron job service.
    /// </summary>
    public virtual void Dispose()
    {
        _executingTask?.Dispose();
        _stoppingCts?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Rounds the given TimeSpan to the nearest second.
    /// </summary>
    /// <param name="timeSpan">The time span to round.</param>
    /// <returns>A TimeSpan rounded to the nearest second.</returns>
    private static TimeSpan RoundToNearestSecond(TimeSpan timeSpan) => TimeSpan.FromSeconds(Math.Ceiling(timeSpan.TotalSeconds));
}