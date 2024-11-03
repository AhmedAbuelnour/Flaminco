using Flaminco.RediPolly.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using StackExchange.Redis;

namespace Flaminco.RediPolly.Abstractions;

/// <summary>
///     Abstract class representing a listener for a Redis channel.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="ChannelListener" /> class.
/// </remarks>
/// <param name="options">The options for Redis channel configuration.</param>
public abstract class ChannelListener(IOptions<RedisChannelConfiguration> options) : BackgroundService
{
    /// <summary>
    ///     The Redis channel that the listener is subscribed to.
    /// </summary>
    protected abstract RedisChannel Channel { get; }

    /// <summary>
    ///     The command flags to be used during subscription and unsubscription.
    /// </summary>
    protected virtual CommandFlags CommandFlags { get; } = CommandFlags.None;


    /// <summary>
    ///     Callback method to be implemented by derived classes.
    /// </summary>
    /// <param name="channel">The Redis channel.</param>
    /// <param name="value">The value received on the channel.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation.</returns>
    protected abstract ValueTask<bool> Callback(RedisChannel channel, RedisValue value,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously marks a message as completed by removing its corresponding key from the Redis store.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message, constrained to implement <see cref="IResilientMessage" />.</typeparam>
    /// <param name="message">
    ///     The message to mark as completed, which must implement the <see cref="IResilientMessage" />
    ///     interface.
    /// </param>
    /// <returns>
    ///     A representing the asynchronous operation, which returns true if the key was successfully deleted from the Redis
    ///     store, otherwise false.
    /// </returns>
    protected async ValueTask<bool> MarkAsCompleted<TMessage>(TMessage message) where TMessage : IResilientMessage
    {
        if (options.Value.ConnectionMultiplexer.GetDatabase() is IDatabase database)
            return await database.KeyDeleteAsync($"RediPolly:{Channel}:{message.ResilientKey}");

        return false;
    }

    /// <summary>
    ///     Executes the background service logic.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Initialize the callback delegate
        Action<RedisChannel, RedisValue> _callbackDelegate = async (channel, value) =>
        {
            await GetCallbackResiliencePipeline()
                .ExecuteAsync(async token => await Callback(channel, value, token), stoppingToken);
        };

        // Create a TaskCompletionSource that will complete when the token is cancelled.
        TaskCompletionSource<bool> unsubscribeCompletionSource = new();

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

    /// <summary>
    ///     Gets the resilience pipeline for the callback.
    /// </summary>
    /// <returns>The resilience pipeline.</returns>
    protected virtual ResiliencePipeline<bool> GetCallbackResiliencePipeline()
    {
        return new ResiliencePipelineBuilder<bool>().AddRetry(new RetryStrategyOptions<bool>
        {
            ShouldHandle = new PredicateBuilder<bool>().HandleResult(x => x == false),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true, // adds a random factor to the delay,
            MaxRetryAttempts = 5,
            Delay = TimeSpan.FromSeconds(3)
        }).Build();
    }


    /// <summary>
    ///     Subscribes to the Redis channel.
    /// </summary>
    /// <param name="callback">The callback method to be invoked when a message is received.</param>
    /// <param name="commandFlags">The command flags for subscription.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    private Task SubscribeAsync(Action<RedisChannel, RedisValue> callback,
        CommandFlags commandFlags = CommandFlags.None)
    {
        if (options.Value.ConnectionMultiplexer.GetSubscriber() is ISubscriber subscriber)
            return subscriber.SubscribeAsync(Channel, callback, commandFlags);

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Unsubscribes from the Redis channel.
    /// </summary>
    /// <param name="callback">The callback method to be invoked when a message is received.</param>
    /// <param name="commandFlags">The command flags for unsubscription.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    private Task UnsubscribeAsync(Action<RedisChannel, RedisValue>? callback = default,
        CommandFlags commandFlags = CommandFlags.None)
    {
        if (options.Value.ConnectionMultiplexer.GetSubscriber() is ISubscriber subscriber)
            return subscriber.UnsubscribeAsync(Channel, callback, commandFlags);
        return Task.CompletedTask;
    }
}