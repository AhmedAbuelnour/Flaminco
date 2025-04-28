using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Collections.Concurrent;

namespace Flaminco.RabbitMQ.AMQP.Services
{
    /// <summary>
    /// Provides RabbitMQ connections to publishers and consumers.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AmqpConnectionProvider"/> class.
    /// </remarks>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AmqpConnectionProvider"/> class.
    /// </remarks>
    /// <param name="factory">The AMQP client settings.</param>
    /// <param name="logger">The logger instance.</param>
    public sealed class AmqpConnectionProvider(IOptions<ConnectionFactory> factory, ILogger<AmqpConnectionProvider> logger) : IAsyncDisposable
    {
        private IConnection? _connection;
        private readonly ConcurrentDictionary<string, IChannel> _channels = new();
        private readonly SemaphoreSlim _connectionLock = new(1, 1);
        private bool _disposed;
        private readonly ConnectionFactory _factory = factory.Value;

        /// <summary>
        /// Gets a RabbitMQ connection instance, creating it if necessary.
        /// </summary>
        public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(AmqpConnectionProvider));

            if (_connection?.IsOpen == true)
                return _connection;

            await _connectionLock.WaitAsync(cancellationToken);

            try
            {
                // Double-check in case another thread created the connection while we were waiting
                if (_connection?.IsOpen == true)
                    return _connection;

                logger.LogInformation("Opening RabbitMQ connection to {Host}", _factory.HostName);

                // Dispose the old connection if it exists
                if (_connection != null)
                {
                    try
                    {
                        _connection.Dispose();
                    }
                    catch
                    {
                    }
                }

                // Create connection asynchronously
                _connection = await _factory.CreateConnectionAsync(cancellationToken);

                // Handle connection shutdown
                _connection.ConnectionShutdownAsync += async (sender, args) =>
                {
                    logger.LogWarning("RabbitMQ connection closed: {Reason}", args.ReplyText);
                };

                return _connection;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create RabbitMQ connection to {Host}", _factory.HostName);

                throw;
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        /// <summary>
        /// Creates a channel for RabbitMQ operations.
        /// </summary>
        public async Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(AmqpConnectionProvider));

            IConnection connection = await GetConnectionAsync(cancellationToken);

            return await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Gets a dedicated channel for a queue and caches it.
        /// </summary>
        public async Task<IChannel> GetChannelForQueueAsync(string queueName, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(AmqpConnectionProvider));

            // If we already have a channel for this queue, return it if still open
            if (_channels.TryGetValue(queueName, out var existingChannel) && existingChannel.IsOpen)
                return existingChannel;

            IChannel channel = await CreateChannelAsync(cancellationToken: cancellationToken);

            // Configure the channel as needed
            channel.ChannelShutdownAsync += async (sender, args) =>
            {
                logger.LogWarning("RabbitMQ channel for queue {QueueName} closed: {Reason}", queueName, args.ReplyText);

                _channels.TryRemove(queueName, out _);
            };

            // Add to our cache
            _channels[queueName] = channel;

            return channel;
        }

        /// <inheritdoc/>

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            // Close all channels
            foreach (IChannel channel in _channels.Values)
            {
                try
                {
                    await channel.CloseAsync();

                    channel.Dispose();
                }
                catch
                {
                    continue;
                }
            }

            _channels.Clear();

            // Close connection
            if (_connection != null)
            {
                try
                {
                    await _connection.CloseAsync();
                }
                catch { }
                try
                {
                    _connection.Dispose();
                }
                catch { }
                _connection = null;
            }

            _connectionLock.Dispose();
            _disposed = true;

            GC.SuppressFinalize(this);
        }
    }
}