using Flaminco.RediPolly.Abstractions;
using Flaminco.RediPolly.Options;
using Microsoft.Extensions.Options;
using Polly;
using StackExchange.Redis;

namespace WebApplication1.BackgroundServices
{
    public class ChannelListenerExample(IOptions<RedisChannelConfiguration> options) : ChannelListener(options)
    {
        protected override RedisChannel Channel { get => RedisChannel.Literal("test-channel"); }

        // Returning true means don't call the retry policy as i'm managing the call even if it is actually failed. 
        // Returning false means run the retry policy even if it is actually worked.
        protected override ValueTask<bool> Callback(RedisChannel channel, RedisValue value, CancellationToken cancellationToken)
        {
            try
            {
                // Your Logic goes here
                return ValueTask.FromResult(true); // turn off the retry
            }
            catch
            {
                return ValueTask.FromResult(false); // turn on the retry policy
            }
        }

        // the default implementation will retry the callback 5 times exponentially each 3 seconds, and has no fallback behavior.
        // You can override this method to implement your own resilience logic.
        protected override ResiliencePipeline<bool> GetCallbackResiliencePipeline()
        {
            return base.GetCallbackResiliencePipeline();
        }

    }
}
