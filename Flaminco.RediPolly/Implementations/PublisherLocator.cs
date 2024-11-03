using Flaminco.RediPolly.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.RediPolly.Implementations;

/// <summary>
///     Represents a service for locating publishers of Redis channels.
/// </summary>
public sealed class PublisherLocator(IServiceProvider serviceProvider) : IPublisherLocator
{
    /// <summary>
    ///     Gets the publisher instance of the specified type.
    /// </summary>
    /// <typeparam name="TPublisher">The type of the publisher.</typeparam>
    /// <returns>The publisher instance, or null if not found.</returns>
    public ChannelPublisher? GetPublisher<TPublisher>() where TPublisher : ChannelPublisher
    {
        return serviceProvider.GetServices<ChannelPublisher>()?.FirstOrDefault(a => a.GetType() == typeof(TPublisher));
    }
}