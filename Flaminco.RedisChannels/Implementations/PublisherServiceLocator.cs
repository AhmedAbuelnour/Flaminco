namespace Flaminco.RedisChannels.Implementations
{
    using Flaminco.RedisChannels.Abstractions;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Linq;

    public sealed class ChannelPublisherLocator(IServiceProvider serviceProvider) : IChannelPublisherLocator
    {
        public ChannelPublisher? GetPublisher<TPublisher>() where TPublisher : ChannelPublisher
        {
            return serviceProvider.GetServices<ChannelPublisher>()?.FirstOrDefault(a => a.GetType() == typeof(TPublisher));
        }
    }
}
