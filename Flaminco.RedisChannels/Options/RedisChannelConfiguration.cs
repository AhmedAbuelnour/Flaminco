namespace Flaminco.RedisChannels.Options
{
    using StackExchange.Redis;

    public sealed class RedisChannelConfiguration
    {
        public required IConnectionMultiplexer ConnectionMultiplexer { get; set; }
    }
}
