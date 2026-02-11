using Flaminco.RabbitMQ.AMQP.Services;
using Flaminco.RabbitMQ.AMQP.Observability;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;
using System.Diagnostics;

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
        var startTimestamp = Stopwatch.GetTimestamp();
        using var activity = RabbitMqDiagnostics.ActivitySource.StartActivity("rabbitmq.healthcheck", ActivityKind.Internal);
        activity?.SetTag("messaging.system", "rabbitmq");
        RabbitMqDiagnostics.HealthChecksTotal.Add(1);

        try
        {
            IConnection connection = await _connectionManager.GetConnectionAsync(cancellationToken);

            if (connection.IsOpen)
            {
                RabbitMqDiagnostics.HealthCheckDurationMs.Record(Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds,
                    new KeyValuePair<string, object?>("status", "healthy"));

                return HealthCheckResult.Healthy(
                    "RabbitMQ connection is healthy",
                    new Dictionary<string, object>
                    {
                        ["Endpoint"] = connection.Endpoint.ToString(),
                        ["IsOpen"] = connection.IsOpen
                    });
            }

            activity?.SetStatus(ActivityStatusCode.Error, "Connection is not open");
            RabbitMqDiagnostics.HealthChecksFailed.Add(1);
            RabbitMqDiagnostics.HealthCheckDurationMs.Record(Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds,
                new KeyValuePair<string, object?>("status", "unhealthy"));

            return HealthCheckResult.Unhealthy("RabbitMQ connection is not open");
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            RabbitMqDiagnostics.HealthChecksFailed.Add(1);
            RabbitMqDiagnostics.HealthCheckDurationMs.Record(Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds,
                new KeyValuePair<string, object?>("status", "failed"));

            return HealthCheckResult.Unhealthy(
                "RabbitMQ connection check failed",
                exception: ex);
        }
    }
}
