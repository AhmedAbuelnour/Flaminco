using Cronos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Flaminco.TickCronos.Abstractions;

/// <summary>
/// Represents an abstract base class for implementing a cron-based background job service.
/// </summary>
/// <param name="cronExpression">The cron expression for scheduling the job.</param>
/// <param name="timeProvider">The time provider used to obtain current time and time zones.</param>
/// <param name="logger">The logger used for logging messages.</param>
public abstract class JobRequest(string? cronExpression, TimeProvider timeProvider, ILogger logger) : IHostedService, IDisposable
{
    private Task? _executingTask;
    private CancellationTokenSource? _stoppingCts;
    private CronExpression? _expression;

    /// <summary>
    /// Gets the name of the cron job.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Handles the job execution.
    /// Override this method to implement custom job processing logic directly within the JobRequest class.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public abstract Task Consume(CancellationToken cancellationToken);

    /// <summary>
    /// Starts the cron job service asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to signal the start process.</param>
    /// <returns>A task representing the start process.</returns>
    public virtual Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("{JobName}: started with expression [{Expression}].", Name, cronExpression);

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
    public virtual ValueTask<DateTimeOffset?> ConfigureNextOccurrence(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(cronExpression))
        {
            _expression ??= CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);

            return ValueTask.FromResult(_expression.GetNextOccurrence(timeProvider.GetUtcNow(), timeProvider.LocalTimeZone));
        }

        return ValueTask.FromResult<DateTimeOffset?>(null);
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
                DateTimeOffset? next = await ConfigureNextOccurrence(cancellationToken);

                if (!next.HasValue)
                {
                    logger.LogInformation("{JobName}: No upcoming schedules, job will not continue", Name);

                    break;
                }

                DateTimeOffset nowTime = timeProvider.GetLocalNow();

                TimeSpan delay = RoundToNearestSecond(next.Value - nowTime);

                if (delay.TotalMilliseconds <= 0)
                {
                    logger.LogInformation("{JobName}: Next occurrence is in the past, skipping...", Name);

                    continue;
                }

                using PeriodicTimer periodicTimer = new(delay);

                await periodicTimer.WaitForNextTickAsync(cancellationToken);  // Wait until the next schedule

                if (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await Consume(cancellationToken);
                        logger.LogInformation("{JobName}: Job executed at {Time}", Name, timeProvider.GetLocalNow());
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "{JobName}: Error occurred during job execution", Name);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                logger.LogInformation(ex, "{JobName}: job received cancellation signal, stopping...", Name);
            }
            catch (Exception e)
            {
                logger.LogError(e, "{JobName}: an error occurred while scheduling the job", Name);
            }
        }
    }

    /// <summary>
    /// Stops the cron job service asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to signal the stop process.</param>
    /// <returns>A task representing the stop process.</returns>
    public virtual async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("{JobName}: stopping...", Name);

        if (_stoppingCts is not null)
        {
            await _stoppingCts.CancelAsync();
        }

        logger.LogInformation("{JobName}: stopped.", Name);
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