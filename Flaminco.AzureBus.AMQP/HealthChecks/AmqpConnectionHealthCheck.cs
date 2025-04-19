using Flaminco.AzureBus.AMQP.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Flaminco.AzureBus.AMQP.HealthChecks
{
    /// <summary>
    /// Implements a health check for AMQP connections to RabbitMQ.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AmqpConnectionHealthCheck"/> class.
    /// </remarks>
    /// <param name="_connectionProvider">The AMQP connection provider to use for health checks.</param>
    public sealed class AmqpConnectionHealthCheck(AmqpConnectionProvider _connectionProvider) : IHealthCheck
    {
        /// <summary>
        /// Checks the health of the AMQP connection.
        /// </summary>
        /// <param name="context">A context object associated with the current health check.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the health check.</param>
        /// <returns>A <see cref="Task{HealthCheckResult}"/> that completes when the health check has finished, yielding the status of the component being checked.</returns>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                Amqp.Connection connection = await _connectionProvider.GetConnectionAsync();

                return connection != null && !connection.IsClosed ? HealthCheckResult.Healthy("AMQP connection is established and working") : HealthCheckResult.Unhealthy("AMQP connection is closed or not working properly");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Failed to establish AMQP connection", ex);
            }
        }
    }
}