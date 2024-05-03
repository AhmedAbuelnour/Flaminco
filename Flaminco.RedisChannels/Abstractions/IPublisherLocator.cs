namespace Flaminco.RedisChannels.Abstractions
{
    public interface IPublisherLocator
    {
        ChannelPublisher? GetPublisher<TPublisher>() where TPublisher : ChannelPublisher;
    }
}
