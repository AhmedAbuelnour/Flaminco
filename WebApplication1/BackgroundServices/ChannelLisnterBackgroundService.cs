using Flaminco.RedisChannels.Abstractions;
using Flaminco.RedisChannels.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace WebApplication1.BackgroundServices
{
    public class ChannelLisnterBackgroundService : ChannelListener
    {
        public ChannelLisnterBackgroundService(IOptions<RedisChannelConfiguration> options) : base(options)
        {

        }
        protected override RedisChannel Channel { get => RedisChannel.Literal("test-channel"); }

        protected override ValueTask Callback(RedisChannel channel, RedisValue value)
        {

            Console.WriteLine($"SubscribeCallback Received message: {value}");

            return ValueTask.CompletedTask;
        }
    }
}
