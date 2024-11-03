namespace Flaminco.RediPolly.Abstractions;

/// <summary>
///     Represents a service for locating publishers of Redis channels.
/// </summary>
public interface IPublisherLocator
{
    /// <summary>
    ///     Gets the publisher instance of the specified type.
    /// </summary>
    /// <typeparam name="TPublisher">The type of the publisher.</typeparam>
    /// <returns>The publisher instance, or null if not found.</returns>
    ChannelPublisher? GetPublisher<TPublisher>() where TPublisher : ChannelPublisher;
}