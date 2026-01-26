namespace Flaminco.RabbitMQ.AMQP.Serialization;

/// <summary>
/// Provides message serialization and deserialization for RabbitMQ messages.
/// </summary>
public interface IMessageSerializer
{
    /// <summary>
    /// Gets the content type for serialized messages (e.g., "application/json").
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// Serializes a message to a byte array.
    /// </summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <param name="message">The message to serialize.</param>
    /// <returns>The serialized message as a byte array.</returns>
    byte[] Serialize<T>(T message);

    /// <summary>
    /// Deserializes a message from a byte array.
    /// </summary>
    /// <typeparam name="T">The expected message type.</typeparam>
    /// <param name="data">The serialized message data.</param>
    /// <returns>The deserialized message.</returns>
    T? Deserialize<T>(ReadOnlySpan<byte> data);

    /// <summary>
    /// Deserializes a message from a byte array to a specific type.
    /// </summary>
    /// <param name="data">The serialized message data.</param>
    /// <param name="type">The target type.</param>
    /// <returns>The deserialized message.</returns>
    object? Deserialize(ReadOnlySpan<byte> data, Type type);
}
