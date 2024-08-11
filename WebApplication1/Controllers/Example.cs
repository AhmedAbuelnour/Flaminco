using Flaminco.RabbitMQ.AMQP.Abstractions;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Publishers;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Example(IAMQPLocator _amqpLocator)
    {
        [HttpGet]
        public async Task PushMessage(CancellationToken cancellationToken)
        {
            await using MessagePublisher helloPublisher = _amqpLocator.GetPublisher<PersonPublisher>();

            await helloPublisher.PublishAsync(new Person
            {
                Name = "Ahmed Abuelnour",
                Age = 30
            }, cancellationToken);
        }
    }

    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
