namespace Flaminco.RedisChannels.Implementations
{
    using Flaminco.RedisChannels.Abstractions;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Linq;

    public sealed class PublisherLocator(IServiceProvider serviceProvider) : IPublisherLocator
    {
        public ChannelPublisher? GetPublisher<TPublisher>() where TPublisher : ChannelPublisher
        {
            return serviceProvider.GetServices<ChannelPublisher>()?.FirstOrDefault(a => a.GetType() == typeof(TPublisher));
        }
    }
}
