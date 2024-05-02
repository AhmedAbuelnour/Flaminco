using StackExchange.Redis;

namespace Flaminco.RedisChannels.Options
{
    public class RedisChannelConfiguration
    {
        public required IConnectionMultiplexer ConnectionMultiplexer { get; set; }
    }
}
