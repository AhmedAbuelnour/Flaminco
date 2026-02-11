using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Flaminco.RabbitMQ.AMQP.Observability;

internal static class RabbitMqDiagnostics
{
    public const string ActivitySourceName = "Flaminco.RabbitMQ.AMQP";
    public const string MeterName = "Flaminco.RabbitMQ.AMQP";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName, "1.0.0");
    public static readonly Meter Meter = new(MeterName, "1.0.0");

    public static readonly Counter<long> ConsumerMessagesProcessed =
        Meter.CreateCounter<long>("rabbitmq.consumer.messages.processed", unit: "messages");

    public static readonly Counter<long> ConsumerMessagesFailed =
        Meter.CreateCounter<long>("rabbitmq.consumer.messages.failed", unit: "messages");

    public static readonly Counter<long> ConsumerMessagesRetried =
        Meter.CreateCounter<long>("rabbitmq.consumer.messages.retried", unit: "messages");

    public static readonly Histogram<double> ConsumerProcessingDurationMs =
        Meter.CreateHistogram<double>("rabbitmq.consumer.processing.duration", unit: "ms");

    public static readonly Counter<long> HealthChecksTotal =
        Meter.CreateCounter<long>("rabbitmq.healthcheck.total", unit: "checks");

    public static readonly Counter<long> HealthChecksFailed =
        Meter.CreateCounter<long>("rabbitmq.healthcheck.failed", unit: "checks");

    public static readonly Histogram<double> HealthCheckDurationMs =
        Meter.CreateHistogram<double>("rabbitmq.healthcheck.duration", unit: "ms");
}
