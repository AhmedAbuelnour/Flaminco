using StackExchange.Redis;
using System.Reflection;

namespace Flaminco.RedisChannels.Options;

/// <summary>
///     Represents the configuration options for a Redis Stream.
/// </summary>
public sealed class RedisStreamConfiguration
{
    /// <summary>
    ///     Gets or sets the Redis connection multiplexer.
    /// </summary>
    /// <remarks>
    ///     This property is required for configuring the Redis stream.
    /// </remarks>
    public required IConnectionMultiplexer ConnectionMultiplexer { get; set; }

    /// <summary>
    ///     Gets or sets the default consumer group name. Used for multi-pod/consumer scenarios.
    ///     If not specified, a default group will be created per stream.
    /// </summary>
    public string? DefaultConsumerGroupName { get; set; }

    /// <summary>
    ///     Gets or sets the consumer name. Used to identify this consumer instance in a consumer group.
    ///     If not specified, a unique name will be generated.
    /// </summary>
    public string? ConsumerName { get; set; }

    /// <summary>
    ///     Gets or sets the maximum length of the stream before trimming old entries.
    ///     If null, the stream will not be automatically trimmed.
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    ///     Gets or sets the block time in milliseconds when reading from the stream.
    ///     Default is 5000ms (5 seconds).
    /// </summary>
    public int BlockTimeMs { get; set; } = 5000;

    /// <summary>
    ///     Gets or sets the number of messages to read per batch.
    ///     Default is 1.
    /// </summary>
    public int BatchSize { get; set; } = 1;

    /// <summary>
    ///     Gets or sets whether to read pending messages (unacknowledged messages) first before reading new messages.
    ///     This is useful for recovering from crashes. Default is true.
    /// </summary>
    public bool ReadPendingMessagesFirst { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether to automatically acknowledge messages after they are successfully read.
    ///     When true, messages are acknowledged immediately after being read. When false, manual acknowledgment is required.
    ///     Default is true.
    /// </summary>
    public bool AutoAcknowledge { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether channel payloads should include runtime type metadata when needed.
    ///     This is useful when using interface/abstract message contracts (for example IDomainEvent).
    ///     Default is true.
    /// </summary>
    public bool EnablePolymorphicSerialization { get; set; } = true;

    /// <summary>
    ///     Gets the additional assemblies that should be searched when resolving envelope runtime types.
    /// </summary>
    public ISet<Assembly> KnownTypeAssemblies { get; } = new HashSet<Assembly>();

    /// <summary>
    ///     Registers one or more assemblies that contain message runtime types.
    /// </summary>
    /// <param name="assemblies">Assemblies to register for runtime type resolution.</param>
    /// <returns>The current configuration instance.</returns>
    public RedisStreamConfiguration AddKnownTypeAssemblies(params Assembly[] assemblies)
    {
        foreach (Assembly assembly in assemblies.Where(a => a is not null))
            KnownTypeAssemblies.Add(assembly);

        return this;
    }
}
