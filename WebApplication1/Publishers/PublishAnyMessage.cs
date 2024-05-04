using Flaminco.RediPolly.Abstractions;
using Flaminco.RediPolly.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace WebApplication1.Publishers
{
    public class PublishAnyMessage(IOptions<RedisChannelConfiguration> options) : ChannelPublisher(options)
    {
        protected override RedisChannel Channel => RedisChannel.Literal("test-channel");
    }
}
