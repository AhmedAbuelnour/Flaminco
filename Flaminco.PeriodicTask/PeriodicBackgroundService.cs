using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Flaminco.PeriodicTask
{
    /// <summary>
    /// Provides an implementation of a periodic background service for removing log items. This service inherits from the
    /// PeriodicBackgroundService abstract class, utilizing its scheduling and execution framework to periodically perform
    /// cleanup tasks. The service's execution period, enabling status, and last run timestamp are customizable. Implementations
    /// should override the RunAsync method to specify the task logic, such as removing old log entries from a database or file
    /// system, based on the service's schedule.
    /// </summary>
    public abstract class PeriodicBackgroundService : BackgroundService
    {
        private readonly ILogger<PeriodicBackgroundService> _logger;
        /// <summary>
        /// Initializes a new instance of the PeriodicBackgroundService class. This constructor is designed to be called by
        /// derived classes, providing them with the necessary infrastructure to log information, warnings, and errors encountered
        /// during the execution of the background service. The ILogger instance passed to the constructor enables the derived
        /// services to utilize built-in logging functionality to record operational data, making it easier to monitor, debug,
        /// and maintain the service.
        /// </summary>
        /// <param name="logger">The logger used to log messages for the background service. This logger is specific to the
        /// derived service type, allowing for more granular control over logging settings and output.</param>
        public PeriodicBackgroundService(ILogger<PeriodicBackgroundService> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// Gets the name of the background service. This name is used for logging purposes to identify the service in logs.
        /// Derived classes must override this property to provide a unique name for each service implementation.
        /// </summary>
        protected abstract string Name { get; }
        /// <summary>
        /// Determines whether the background service is enabled and should run according to its schedule. This can be used
        /// to dynamically enable or disable the service based on external conditions or application settings. Derived classes
        /// must provide an implementation to control the service's enabled status.
        /// </summary>
        protected abstract bool IsEnabled { get; set; }
        /// <summary>
        /// Specifies the Period between consecutive executions of the background service. This period defines how often the
        /// service will run, starting from the completion of the last execution. Derived classes must override this property
        /// to set the frequency of execution according to the service's needs.
        /// </summary>
        protected abstract TimeSpan Period { get; }
        /// <summary>
        /// Contains the logic that the background service will execute at each scheduled interval. This method is called
        /// by the ExecuteAsync method according to the service's period and enabled status. Derived classes must implement
        /// this method to perform the service's specific tasks, such as processing data, cleaning up resources, or any other
        /// periodic background work.
        /// </summary>
        /// <param name="stoppingToken">A CancellationToken that should be monitored for cancellation requests to gracefully stop the task.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>

        protected abstract Task RunAsync(CancellationToken stoppingToken);

        /// <summary>
        /// Executes the background task in a loop at the scheduled intervals defined by the Period property. This method calculates
        /// the initial delay before the first execution based on the last run time and the period, ensuring that the task maintains
        /// its schedule even after a restart. Once the initial delay is observed, it enters a loop that continues until the application
        /// stops or the cancellation token is triggered. Within this loop, it checks if the service is enabled before executing the
        /// task. If the service is disabled, it skips the execution cycle. This method handles exceptions by logging them and
        /// continues with the next execution cycle. It uses a PeriodicTimer to manage the scheduling of the task executions.
        /// </summary>
        /// <param name="stoppingToken">A CancellationToken that should be monitored for cancellation requests to gracefully stop the task.</param>
        /// <returns>A Task representing the asynchronous operation, allowing the background service to perform its work indefinitely until stopped.</returns>
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (PeriodicTimer timer = new(Period))
            {
                while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
                {
                    try
                    {
                        if (IsEnabled)
                        {
                            _logger.LogInformation("BackgroundService: {Name} has been started", Name);

                            await RunAsync(stoppingToken);

                            _logger.LogInformation("BackgroundService: {Name} has been completed successfully", Name);
                        }
                        else
                        {
                            _logger.LogInformation("BackgroundService: {Name} has been skipped because the service is not enabled", Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An exception occurred in the BackgroundService: {Name}", Name);
                    }
                }
            }
        }
    }
}

