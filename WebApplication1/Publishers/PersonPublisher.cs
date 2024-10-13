//using Flaminco.AzureBus.AMQP.Abstractions;
//using Flaminco.AzureBus.AMQP.Options;
//using MediatR;
//using Microsoft.Extensions.Options;

//namespace WebApplication1.Publishers
//{
//    public class HelloConsumer : MessageConsumer
//    {
//        public HelloConsumer(IOptions<AMQPClientSettings> addressSettings, IPublisher publisher) : base(addressSettings, publisher)
//        {
//        }

//        protected override string Name => "hello-consumer";
//        protected override string Queue => "hello";
//    }

//    public class PersonPublisher : MessagePublisher
//    {
//        public PersonPublisher(IOptions<AMQPClientSettings> addressSettings) : base(addressSettings)
//        {

//        }

//        protected override string Name => "nameof(PersonPublisher)";
//        protected override string[] Queues => ["hello"];
//    }

//    public class HelloConsumer2 : MessageConsumer
//    {
//        public HelloConsumer2(IOptions<AMQPClientSettings> addressSettings, IPublisher publisher) : base(addressSettings, publisher)
//        {
//        }

//        protected override string Name => "hello-consumer2";
//        protected override string Queue => "hello2";
//    }

//    public class PersonPublisher2 : MessagePublisher
//    {
//        public PersonPublisher2(IOptions<AMQPClientSettings> addressSettings) : base(addressSettings)
//        {

//        }

//        protected override string Name => "nameof(PersonPublisher2)";
//        protected override string[] Queues => ["hello2"];
//    }
//}

