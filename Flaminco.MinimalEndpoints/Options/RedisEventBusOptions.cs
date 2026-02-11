using StackExchange.Redis;

namespace Flaminco.MinimalEndpoints.Options
{
    /// <summary>
    /// Options for Redis-backed EventBus transport.
    /// </summary>
    public sealed class RedisEventBusOptions
    {
        /// <summary>
        /// Redis connection string.
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Redis list key used as the event queue.
        /// </summary>
        public string QueueKey { get; set; } = "domain-events";

        /// <summary>
        /// Blocking pop timeout in seconds. Defaults to 5.
        /// </summary>
        public int PopTimeoutSeconds { get; set; } = 5;

        /// <summary>
        /// Connection multiplexer override. If supplied, ConnectionString is ignored.
        /// </summary>
        public IConnectionMultiplexer? ConnectionMultiplexer { get; set; }
    }
}

