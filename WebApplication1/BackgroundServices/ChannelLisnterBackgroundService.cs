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

        protected override ValueTask<bool> Callback(RedisChannel channel, RedisValue value, CancellationToken cancellationToken)
        {
            try
            {

                return ValueTask.FromResult(true);
            }
            catch
            {
                return ValueTask.FromResult(false);
            }
        }

    }
}
