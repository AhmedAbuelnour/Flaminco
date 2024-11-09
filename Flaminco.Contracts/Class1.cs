using Flaminco.RabbitMQ.AMQP.Abstractions;

namespace Flaminco.Contracts;

public class ExampleRequest : IMessage
{
    public int Id { get; set; }
}

public class ExampleResponse : IMessage
{
    public string Message { get; set; }
}