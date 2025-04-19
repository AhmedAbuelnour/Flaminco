using Amqp;
using Amqp.Framing;
using Flaminco.RabbitMQ.AMQP.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Flaminco.RabbitMQ.AMQP.Services
{
    /// <summary>
    /// Provides AMQP connections to publishers and consumers.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AmqpConnectionProvider"/> class.
    /// </remarks>
    public sealed class AmqpConnectionProvider(IOptions<AmqpClientSettings> settings, ILogger<AmqpConnectionProvider> logger) : IDisposable
    {
        private readonly AmqpClientSettings _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        private Connection? _connection;
        private readonly ConcurrentDictionary<string, Session> _sessions = new();
        private readonly SemaphoreSlim _connectionLock = new(1, 1);
        private bool _disposed;

        /// <summary>
        /// Gets an AMQP connection instance, creating it if necessary.
        /// </summary>
        public async Task<Connection> GetConnectionAsync()
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(AmqpConnectionProvider));

            if (_connection != null && !_connection.IsClosed)
                return _connection;

            await _connectionLock.WaitAsync();

            try
            {
                // Double-check in case another thread created the connection while we were waiting
                if (_connection != null && !_connection.IsClosed)
                    return _connection;

                logger.LogInformation("Opening AMQP connection to {Host}", _settings.Host);

                // Create connection object with address and connection settings
                Address address = new(_settings.Host);

                ConnectionFactory connectionFactory = new();

                // Set connection properties
                Connection connection = await connectionFactory.CreateAsync(address, new Open()
                {
                    ContainerId = Guid.NewGuid().ToString(),
                    IdleTimeOut = (uint)_settings.IdleTimeout.TotalMilliseconds
                });

                connection.Closed += (sender, error) =>
                {
                    logger.LogWarning("AMQP connection closed: {Error}", error?.ToString() ?? "Unknown reason");
                };

                _connection = connection;

                return connection;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create AMQP connection to {Host}", _settings.Host);

                throw;
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        /// <summary>
        /// Gets an AMQP session for the specified queue.
        /// </summary>
        public async Task<Session> GetSessionAsync(string queueName)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(AmqpConnectionProvider));

            // If we already have a session for this queue, return it
            if (_sessions.TryGetValue(queueName, out var existingSession) && !existingSession.IsClosed)
                return existingSession;

            // Otherwise create a new session
            Connection connection = await GetConnectionAsync();

            // Create a new session on the connection
            Session session = new(connection);

            session.Closed += (sender, error) =>
            {
                logger.LogWarning("AMQP session for queue {QueueName} closed: {Error}", queueName, error?.ToString() ?? "Unknown reason");

                _sessions.TryRemove(queueName, out _);
            };

            _sessions[queueName] = session;

            return session;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed)
                return;

            // Close all sessions
            foreach (var session in _sessions.Values)
            {
                try { session.Close(); } catch { }
            }

            _sessions.Clear();

            // Close connection
            if (_connection != null)
            {
                try { _connection.Close(); }
                catch { }
                _connection = null!; // Non-nullable field suppression with null-forgiving operator
            }

            _connectionLock.Dispose();
            _disposed = true;

            GC.SuppressFinalize(this);
        }
    }
}