using RabbitMQ.Client;

namespace Flaminco.RabbitMQ.AMQP.Abstractions;

/// <summary>
/// Provides context information about a consumed message.
/// </summary>
/// <typeparam name="TMessage">The message type.</typeparam>
public sealed class ConsumeContext<TMessage>
{
    /// <summary>
    /// Gets the deserialized message.
    /// </summary>
    public required TMessage Message { get; init; }

    /// <summary>
    /// Gets the raw message body as bytes.
    /// </summary>
    public required ReadOnlyMemory<byte> Body { get; init; }

    /// <summary>
    /// Gets the message properties.
    /// </summary>
    public required IReadOnlyBasicProperties Properties { get; init; }

    /// <summary>
    /// Gets the delivery tag for acknowledgment.
    /// </summary>
    public required ulong DeliveryTag { get; init; }

    /// <summary>
    /// Gets the exchange from which the message was published.
    /// </summary>
    public required string Exchange { get; init; }

    /// <summary>
    /// Gets the routing key used to route the message.
    /// </summary>
    public required string RoutingKey { get; init; }

    /// <summary>
    /// Gets whether this message was redelivered.
    /// </summary>
    public required bool Redelivered { get; init; }

    /// <summary>
    /// Gets the consumer tag.
    /// </summary>
    public required string ConsumerTag { get; init; }

    /// <summary>
    /// Gets the message ID if present.
    /// </summary>
    public string? MessageId => Properties.MessageId;

    /// <summary>
    /// Gets the correlation ID if present.
    /// </summary>
    public string? CorrelationId => Properties.CorrelationId;

    /// <summary>
    /// Gets the current retry attempt (0 for first attempt).
    /// </summary>
    public int RetryCount { get; init; }

    /// <summary>
    /// Gets the timestamp when the message was published.
    /// </summary>
    public DateTimeOffset? Timestamp => Properties.Timestamp.UnixTime > 0
        ? DateTimeOffset.FromUnixTimeSeconds(Properties.Timestamp.UnixTime)
        : null;

    /// <summary>
    /// Gets a header value by key.
    /// </summary>
    /// <typeparam name="T">The expected header value type.</typeparam>
    /// <param name="key">The header key.</param>
    /// <returns>The header value or default.</returns>
    public T? GetHeader<T>(string key)
    {
        if (Properties.Headers?.TryGetValue(key, out var value) == true && value is T typedValue)
            return typedValue;
        return default;
    }

    /// <summary>
    /// Gets a header value as a string.
    /// </summary>
    /// <param name="key">The header key.</param>
    /// <returns>The header value as string or null.</returns>
    public string? GetHeaderString(string key)
    {
        if (Properties.Headers?.TryGetValue(key, out var value) == true)
        {
            return value switch
            {
                byte[] bytes => System.Text.Encoding.UTF8.GetString(bytes),
                string str => str,
                _ => value?.ToString()
            };
        }
        return null;
    }
}
