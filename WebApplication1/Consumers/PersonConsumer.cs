using Flaminco.RabbitMQ.AMQP.Abstractions;
using Flaminco.RabbitMQ.AMQP.Options;
using Microsoft.Extensions.Options;

namespace WebApplication1.Consumers
{
    public class PersonConsumer(IOptions<AddressSettings> _addressSettings) : MessageConsumer(_addressSettings)
    {
        protected override ValueTask<string> GetKeyAsync(CancellationToken cancellationToken = default)
        {
            // a key or name for this current consumer, which is used for logs 
            return ValueTask.FromResult(nameof(PersonConsumer));
        }

        protected override ValueTask<string> GetQueueAsync(CancellationToken cancellationToken = default)
        {
            // the queue name which this consumer will receive the messages from.
            return ValueTask.FromResult("HelloQueue");
        }
    }
}
