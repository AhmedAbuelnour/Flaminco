using MediatR;
using System.Threading.Channels;

namespace Flaminco.MinimalMediatR.Implementations
{
    internal sealed class InMemoryMessageQueue
    {
        private readonly Channel<INotification> _channel = Channel.CreateUnbounded<INotification>();

        internal ChannelWriter<INotification> Writer => _channel.Writer;
        internal ChannelReader<INotification> Reader => _channel.Reader;
    }
}
