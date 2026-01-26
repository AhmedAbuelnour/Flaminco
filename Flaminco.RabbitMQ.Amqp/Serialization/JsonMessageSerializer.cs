using System.Text.Json;

namespace Flaminco.RabbitMQ.AMQP.Serialization;

/// <summary>
/// Default JSON message serializer using System.Text.Json.
/// </summary>
public sealed class JsonMessageSerializer : IMessageSerializer
{
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// Initializes a new instance with default options.
    /// </summary>
    public JsonMessageSerializer() : this(JsonSerializerOptions.Web)
    {
    }

    /// <summary>
    /// Initializes a new instance with custom options.
    /// </summary>
    /// <param name="options">The JSON serializer options.</param>
    public JsonMessageSerializer(JsonSerializerOptions options)
    {
        _options = options;
    }

    /// <inheritdoc />
    public string ContentType => "application/json";

    /// <inheritdoc />
    public byte[] Serialize<T>(T message)
    {
        return JsonSerializer.SerializeToUtf8Bytes(message, _options);
    }

    /// <inheritdoc />
    public T? Deserialize<T>(ReadOnlySpan<byte> data)
    {
        return JsonSerializer.Deserialize<T>(data, _options);
    }

    /// <inheritdoc />
    public object? Deserialize(ReadOnlySpan<byte> data, Type type)
    {
        return JsonSerializer.Deserialize(data, type, _options);
    }
}
