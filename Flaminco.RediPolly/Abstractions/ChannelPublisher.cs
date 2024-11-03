using System.Text.Json;
using Flaminco.RediPolly.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Flaminco.RediPolly.Abstractions;

/// <summary>
///     Represents a base class for publishing messages to Redis channels.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="ChannelPublisher" /> class.
/// </remarks>
/// <param name="options">The options for Redis channel configuration.</param>
/// <exception cref="InvalidOperationException">Thrown when the connection multiplexer is not set or not connected.</exception>
public abstract class ChannelPublisher(IOptions<RedisChannelConfiguration> options)
{
    /// <summary>
    ///     The Redis channel to which messages are published.
    /// </summary>
    protected abstract RedisChannel Channel { get; }

    /// <summary>
    ///     Publishes a message to the Redis channel.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message to publish.</typeparam>
    /// <param name="message">The message to publish.</param>
    /// <param name="jsonOptions">The options for JSON serialization.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task PublishAsync<TMessage>(TMessage message, JsonSerializerOptions? jsonOptions = null)
    {
        if (options.Value.ConnectionMultiplexer.GetSubscriber() is ISubscriber subscriber)
            return subscriber.PublishAsync(Channel, JsonSerializer.SerializeToUtf8Bytes(message, jsonOptions));

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Publishes a resilient message to the Redis channel which keep track of this message in the RediPolly channel.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message to publish.</typeparam>
    /// <param name="message">The message to publish.</param>
    /// <param name="jsonOptions">The options for JSON serialization.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ResilientPublishAsync<TMessage>(TMessage message, JsonSerializerOptions? jsonOptions = null)
        where TMessage : IResilientMessage
    {
        if (options.Value.ConnectionMultiplexer.GetSubscriber() is ISubscriber subscriber)
        {
            var messageBytes = JsonSerializer.SerializeToUtf8Bytes(message, jsonOptions);

            if (options.Value.ConnectionMultiplexer.GetDatabase() is IDatabase database)
                database.StringSetAsync($"RediPolly:{Channel}:{message.ResilientKey}", messageBytes,
                    options.Value.ResilientExpiry);

            return subscriber.PublishAsync(Channel, messageBytes);
        }

        return Task.CompletedTask;
    }
}