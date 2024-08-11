using Flaminco.RabbitMQ.AMQP.Abstractions;
using Flaminco.RabbitMQ.AMQP.Options;
using Microsoft.Extensions.Options;

namespace WebApplication1.Publishers
{
    public class PersonPublisher : MessagePublisher
    {

        public PersonPublisher(IOptions<AddressSettings> _addressSettings) : base(_addressSettings)
        {

        }

        protected override ValueTask<string> GetKeyAsync(CancellationToken cancellationToken = default)
        {
            // a key or name for this current publisher, which is used for logs 

            return ValueTask.FromResult(nameof(PersonPublisher));
        }

        protected override ValueTask<string[]> GetQueuesAsync(CancellationToken cancellationToken = default)
        {
            // the queue name which this publisher will send the messages to.
            // this publisher can send same message to multiple queues for different consumers.

            return ValueTask.FromResult<string[]>(["HelloQueue"]);
        }
    }
}
