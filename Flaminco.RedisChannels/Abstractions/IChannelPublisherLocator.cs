namespace Flaminco.RedisChannels.Abstractions
{
    public interface IChannelPublisherLocator
    {
        ChannelPublisher? GetPublisher<TPublisher>() where TPublisher : ChannelPublisher;
    }
}
