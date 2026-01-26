using Flaminco.RabbitMQ.AMQP.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Flaminco.RabbitMQ.AMQP.HealthChecks;

/// <summary>
/// Health check for RabbitMQ connection status.
/// </summary>
internal sealed class RabbitMqHealthCheck : IHealthCheck
{
    private readonly IRabbitMqConnectionManager _connectionManager;

    public RabbitMqHealthCheck(IRabbitMqConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = await _connectionManager.GetConnectionAsync(cancellationToken);

            if (connection.IsOpen)
            {
                return HealthCheckResult.Healthy(
                    "RabbitMQ connection is healthy",
                    new Dictionary<string, object>
                    {
                        ["Endpoint"] = connection.Endpoint.ToString(),
                        ["IsOpen"] = connection.IsOpen
                    });
            }

            return HealthCheckResult.Unhealthy("RabbitMQ connection is not open");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "RabbitMQ connection check failed",
                exception: ex);
        }
    }
}
