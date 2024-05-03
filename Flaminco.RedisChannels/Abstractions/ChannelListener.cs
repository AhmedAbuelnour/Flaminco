namespace Flaminco.RedisChannels.Abstractions
{
    using Flaminco.RedisChannels.Options;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using StackExchange.Redis;
    public abstract class ChannelListener : BackgroundService
    {
        protected abstract RedisChannel Channel { get; }
        protected abstract ValueTask Callback(RedisChannel channel, RedisValue value);

        protected virtual CommandFlags CommandFlags { get; } = CommandFlags.None;

        private readonly IConnectionMultiplexer _connectionMultiplexer;

        protected ChannelListener(IOptions<RedisChannelConfiguration> options)
        {
            if (options.Value.ConnectionMultiplexer is null)
            {
                throw new InvalidOperationException("Please set the connection multiplexer");
            }

            if (!options.Value.ConnectionMultiplexer.IsConnected)
            {
                throw new InvalidOperationException("Can't Start a publisher when the Redis connections isn't started yet, please make sure to start it first");
            }

            _connectionMultiplexer = options.Value.ConnectionMultiplexer;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Initialize the callback delegate
            Action<RedisChannel, RedisValue> _callbackDelegate = async (channel, value) => await Callback(channel, value);

            // Create a TaskCompletionSource that will complete when the token is cancelled.
            TaskCompletionSource<bool> unsubscribeCompletionSource = new TaskCompletionSource<bool>();

            // Register a callback to set the TaskCompletionSource when cancellation is requested.
            using (stoppingToken.Register(() => unsubscribeCompletionSource.SetResult(true)))
            {
                // Subscribe once when the task starts.
                await SubscribeAsync(_callbackDelegate, CommandFlags);

                // Wait until the cancellation token is triggered.
                await unsubscribeCompletionSource.Task;

                // Unsubscribe when the cancellationToken is requested.
                await UnsubscribeAsync(_callbackDelegate, CommandFlags);
            }
        }

        private Task SubscribeAsync(Action<RedisChannel, RedisValue> callback, CommandFlags commandFlags = CommandFlags.None)
        {
            if (_connectionMultiplexer.GetSubscriber() is ISubscriber subscriber)
            {
                return subscriber.SubscribeAsync(Channel, callback, commandFlags);
            }

            return Task.CompletedTask;
        }

        private Task UnsubscribeAsync(Action<RedisChannel, RedisValue>? callback = default, CommandFlags commandFlags = CommandFlags.None)
        {
            if (_connectionMultiplexer.GetSubscriber() is ISubscriber subscriber)
            {
                return subscriber.UnsubscribeAsync(Channel, callback, commandFlags);
            }
            return Task.CompletedTask;
        }
    }

}
