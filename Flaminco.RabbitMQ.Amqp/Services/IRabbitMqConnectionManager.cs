using RabbitMQ.Client;

namespace Flaminco.RabbitMQ.AMQP.Services;

/// <summary>
/// Manages RabbitMQ connections and channels with automatic recovery support.
/// </summary>
public interface IRabbitMqConnectionManager : IAsyncDisposable
{
    /// <summary>
    /// Gets a healthy RabbitMQ connection, creating one if necessary.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An open RabbitMQ connection.</returns>
    Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new channel for RabbitMQ operations.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A new channel.</returns>
    Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a channel with publisher confirms enabled.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A channel with publisher confirms enabled.</returns>
    Task<IChannel> CreateConfirmChannelAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets or creates a dedicated channel for a specific purpose (e.g., queue consumption).
    /// </summary>
    /// <param name="key">A unique key identifying the channel purpose.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A dedicated channel.</returns>
    Task<IChannel> GetOrCreateChannelAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets whether the connection is currently open.
    /// </summary>
    bool IsConnected { get; }
}
