using Flaminco.RabbitMQ.AMQP.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Flaminco.RabbitMQ.AMQP.Services;

/// <summary>
/// Default implementation of <see cref="IRabbitMqConnectionManager"/>.
/// </summary>
internal sealed class RabbitMqConnectionManager : IRabbitMqConnectionManager
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqConnectionManager> _logger;
    private readonly ConcurrentDictionary<string, IChannel> _channels = new();
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private IConnection? _connection;
    private bool _disposed;

    public RabbitMqConnectionManager(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqConnectionManager> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public bool IsConnected => _connection?.IsOpen == true;

    /// <inheritdoc />
    public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_connection?.IsOpen == true)
            return _connection;

        await _connectionLock.WaitAsync(cancellationToken);

        try
        {
            // Double-check pattern
            if (_connection?.IsOpen == true)
                return _connection;

            _logger.LogInformation("Creating RabbitMQ connection to {Host}:{Port}/{VirtualHost}",
                _options.HostName, _options.Port, _options.VirtualHost);

            // Dispose old connection if exists
            if (_connection is not null)
            {
                try { _connection.Dispose(); } catch { /* Ignore disposal errors */ }
            }

            ConnectionFactory factory = CreateConnectionFactory();

            // Create connection with endpoints if configured for clustering
            if (_options.Endpoints.Count > 0)
            {
                var endpoints = _options.Endpoints
                    .Select(e => new AmqpTcpEndpoint(e.HostName, e.Port))
                    .ToList();

                _connection = await factory.CreateConnectionAsync(endpoints, cancellationToken);
            }
            else
            {
                _connection = await factory.CreateConnectionAsync(cancellationToken);
            }

            // Set up connection event handlers
            _connection.ConnectionShutdownAsync += OnConnectionShutdownAsync;
            _connection.RecoverySucceededAsync += OnRecoverySucceededAsync;
            _connection.ConnectionRecoveryErrorAsync += OnConnectionRecoveryErrorAsync;

            _logger.LogInformation("RabbitMQ connection established successfully");

            return _connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create RabbitMQ connection to {Host}:{Port}",
                _options.HostName, _options.Port);
            throw;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var connection = await GetConnectionAsync(cancellationToken);
        return await connection.CreateChannelAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IChannel> CreateConfirmChannelAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var connection = await GetConnectionAsync(cancellationToken);

        // Create channel with publisher confirms enabled
        var options = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true
        );

        var channel = await connection.CreateChannelAsync(options, cancellationToken);

        return channel;
    }

    /// <inheritdoc />
    public async Task<IChannel> GetOrCreateChannelAsync(string key, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Return existing channel if it's still open
        if (_channels.TryGetValue(key, out var existingChannel) && existingChannel.IsOpen)
            return existingChannel;

        var channel = await CreateChannelAsync(cancellationToken);

        // Set up channel shutdown handler
        channel.ChannelShutdownAsync += async (sender, args) =>
        {
            _logger.LogWarning("Channel {Key} shut down: {Reason}", key, args.ReplyText);
            _channels.TryRemove(key, out _);
            await Task.CompletedTask;
        };

        _channels[key] = channel;
        return channel;
    }

    private ConnectionFactory CreateConnectionFactory()
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            VirtualHost = _options.VirtualHost,
            UserName = _options.UserName,
            Password = _options.Password,
            AutomaticRecoveryEnabled = _options.AutomaticRecoveryEnabled,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(_options.NetworkRecoveryIntervalSeconds),
            RequestedHeartbeat = TimeSpan.FromSeconds(_options.RequestedHeartbeat),
            ContinuationTimeout = TimeSpan.FromSeconds(_options.ConnectionTimeoutSeconds),
            ClientProvidedName = _options.ClientProvidedName ?? $"Flaminco.RabbitMQ-{Environment.MachineName}"
        };

        // Configure SSL if enabled
        if (_options.Ssl?.Enabled == true)
        {
            factory.Ssl = new SslOption
            {
                Enabled = true,
                ServerName = _options.Ssl.ServerName ?? _options.HostName,
                AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNameMismatch |
                                        SslPolicyErrors.RemoteCertificateChainErrors
            };

            if (!string.IsNullOrEmpty(_options.Ssl.CertificatePath))
            {
                var cert = string.IsNullOrEmpty(_options.Ssl.CertificatePassword)
                    ? X509CertificateLoader.LoadCertificateFromFile(_options.Ssl.CertificatePath)
                    : X509CertificateLoader.LoadPkcs12FromFile(_options.Ssl.CertificatePath, _options.Ssl.CertificatePassword);
                factory.Ssl.Certs = [cert];
            }
        }

        return factory;
    }

    private Task OnConnectionShutdownAsync(object sender, ShutdownEventArgs args)
    {
        _logger.LogWarning("RabbitMQ connection shut down: {ReplyCode} - {ReplyText}",
            args.ReplyCode, args.ReplyText);
        return Task.CompletedTask;
    }

    private Task OnRecoverySucceededAsync(object sender, AsyncEventArgs args)
    {
        _logger.LogInformation("RabbitMQ connection recovered successfully");
        return Task.CompletedTask;
    }

    private Task OnConnectionRecoveryErrorAsync(object sender, ConnectionRecoveryErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "RabbitMQ connection recovery failed");
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        _disposed = true;

        // Close all channels
        foreach (var channel in _channels.Values)
        {
            try
            {
                await channel.CloseAsync();
                channel.Dispose();
            }
            catch { /* Ignore errors during disposal */ }
        }
        _channels.Clear();

        // Close connection
        if (_connection is not null)
        {
            try
            {
                await _connection.CloseAsync();
                _connection.Dispose();
            }
            catch { /* Ignore errors during disposal */ }
            _connection = null;
        }

        _connectionLock.Dispose();
    }
}
