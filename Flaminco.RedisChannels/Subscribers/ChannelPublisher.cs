using Flaminco.RedisChannels.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace Flaminco.RedisChannels.Subscribers
{
    public abstract class ChannelPublisher
    {
        protected abstract RedisChannel Channel { get; }
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        protected ChannelPublisher(IOptions<RedisChannelConfiguration> options)
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

        public Task PublishAsync<TMessage>(TMessage message, JsonSerializerOptions? options = null)
        {
            if (_connectionMultiplexer.GetSubscriber() is ISubscriber subscriber)
            {
                return subscriber.PublishAsync(Channel, JsonSerializer.SerializeToUtf8Bytes(message, options), CommandFlags.None);
            }

            return Task.CompletedTask;
        }
    }

}
