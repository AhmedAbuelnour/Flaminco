namespace Flaminco.RabbitMQ.AMQP.Configuration;

/// <summary>
/// Configuration options for RabbitMQ connection and behavior.
/// </summary>
public sealed class RabbitMqOptions
{
    /// <summary>
    /// The configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "RabbitMQ";

    /// <summary>
    /// Gets or sets the RabbitMQ host name or IP address.
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// Gets or sets the RabbitMQ port. Default is 5672.
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// Gets or sets the virtual host. Default is "/".
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Gets or sets the username for authentication.
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// Gets or sets the password for authentication.
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// Gets or sets the client-provided connection name for identification.
    /// </summary>
    public string? ClientProvidedName { get; set; }

    /// <summary>
    /// Gets or sets whether to enable automatic connection recovery. Default is true.
    /// </summary>
    public bool AutomaticRecoveryEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the network recovery interval in seconds. Default is 5 seconds.
    /// </summary>
    public int NetworkRecoveryIntervalSeconds { get; set; } = 5;

    /// <summary>
    /// Gets or sets the requested heartbeat timeout in seconds. Default is 60 seconds.
    /// </summary>
    public ushort RequestedHeartbeat { get; set; } = 60;

    /// <summary>
    /// Gets or sets the connection timeout in seconds. Default is 30 seconds.
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets whether to enable publisher confirms globally. Default is false.
    /// </summary>
    public bool EnablePublisherConfirms { get; set; } = false;

    /// <summary>
    /// Gets or sets the default prefetch count for consumers. Default is 10.
    /// </summary>
    public ushort DefaultPrefetchCount { get; set; } = 10;

    /// <summary>
    /// Gets or sets SSL/TLS options for secure connections.
    /// </summary>
    public RabbitMqSslOptions? Ssl { get; set; }

    /// <summary>
    /// Gets or sets additional endpoints for clustering support.
    /// </summary>
    public List<RabbitMqEndpoint> Endpoints { get; set; } = [];
}

/// <summary>
/// SSL/TLS configuration options.
/// </summary>
public sealed class RabbitMqSslOptions
{
    /// <summary>
    /// Gets or sets whether SSL is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the server name for SSL certificate validation.
    /// </summary>
    public string? ServerName { get; set; }

    /// <summary>
    /// Gets or sets the path to the client certificate.
    /// </summary>
    public string? CertificatePath { get; set; }

    /// <summary>
    /// Gets or sets the certificate password.
    /// </summary>
    public string? CertificatePassword { get; set; }
}

/// <summary>
/// Represents a RabbitMQ cluster endpoint.
/// </summary>
public sealed class RabbitMqEndpoint
{
    /// <summary>
    /// Gets or sets the host name.
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// Gets or sets the port.
    /// </summary>
    public int Port { get; set; } = 5672;
}
