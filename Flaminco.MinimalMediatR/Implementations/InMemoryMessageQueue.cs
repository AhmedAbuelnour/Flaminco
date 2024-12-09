using MediatR;
using System.Threading.Channels;

namespace Flaminco.MinimalMediatR.Implementations
{
    public sealed class InMemoryMessageQueue
    {
        private readonly Channel<INotification> _channel = Channel.CreateUnbounded<INotification>();

        public ChannelWriter<INotification> Writer => _channel.Writer;
        public ChannelReader<INotification> Reader => _channel.Reader;
    }
}
