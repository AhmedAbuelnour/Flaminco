using Flaminco.RabbitMQ.AMQP.Abstractions;
using Flaminco.RabbitMQ.AMQP.Options;
using MediatR;
using Microsoft.Extensions.Options;

namespace WebApplication1.Publishers
{
    public class HelloConsumer : MessageConsumer
    {
        public HelloConsumer(IOptions<AddressSettings> addressSettings, IPublisher publisher) : base(addressSettings, publisher)
        {
        }

        protected override string Name => nameof(HelloConsumer);
        protected override string Queue => "HelloQueue";
    }

    public class PersonPublisher : MessagePublisher
    {
        public PersonPublisher(IOptions<AddressSettings> addressSettings) : base(addressSettings)
        {
        }

        protected override string Name => nameof(PersonPublisher);
        protected override string[] Queues => ["HelloQueue"];
    }
}
