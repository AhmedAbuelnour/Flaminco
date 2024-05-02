using Flaminco.RedisChannels.Options;
using Flaminco.RedisChannels.Subscribers;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;
using WebApplication1.Controllers;

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

            var message = JsonSerializer.Deserialize<Counter>(value);

            Console.WriteLine($"SubscribeCallback Received message: {message.Count}");



            return ValueTask.CompletedTask;
        }
    }
}
