namespace Flaminco.RediPolly.Options
{
    using StackExchange.Redis;

    /// <summary>
    /// Represents the configuration options for a Redis channel.
    /// </summary>
    public sealed class RedisChannelConfiguration
    {
        /// <summary>
        /// Gets or sets the Redis connection multiplexer.
        /// </summary>
        /// <remarks>
        /// This property is required for configuring the Redis channel.
        /// </remarks>
        public required IConnectionMultiplexer ConnectionMultiplexer { get; set; }
    }
}
