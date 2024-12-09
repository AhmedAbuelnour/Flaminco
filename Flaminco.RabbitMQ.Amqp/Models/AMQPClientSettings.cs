using MassTransit.Configuration;

namespace Flaminco.RabbitMQ.AMQP.Models;

/// <summary>
///     Represents the configuration settings for the AMQP connection, including the host, username, password, and optional
///     retry configurations.
/// </summary>
public class AMQPClientSettings
{
    /// <summary>
    ///     Gets or sets the host address used to connect to the AMQP message broker.
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    ///     Gets or sets the username used for authentication with the AMQP message broker.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    ///     Gets or sets the password used for authentication with the AMQP message broker.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    ///     Gets or sets the number of retry attempts to be made in case of connection failure.
    ///     If null, no retries will be attempted.
    /// </summary>
    public int? RetryCount { get; set; }

    /// <summary>
    ///     Gets or sets the time interval between retry attempts in case of connection failure.
    ///     If null, no retry interval will be applied.
    /// </summary>
    public TimeSpan? RetryInterval { get; set; }

    /// <summary>
    ///     Gets or sets the timeout for sync queue operations
    /// </summary>
    public TimeSpan? SyncQueuePublisherTimeOut { get; set; }


    /// <summary>
    ///     Gets or sets the health check options for the AMQP connection.
    /// </summary>
    public IHealthCheckOptions? HealthCheckOptions { get; set; }
}