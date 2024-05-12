using Flaminco.RediPolly.Abstractions;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Publishers;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Example(IPublisherLocator _locator)
    {
        [HttpGet]
        [Route("publish")]
        public async Task Publish()
        {
            if (_locator.GetPublisher<PublishAnyMessage>() is PublishAnyMessage redisPublisher)
            {
                await redisPublisher.ResilientPublishAsync(new Counter
                {
                    Count = 5
                });
            }
        }
    }


    public class Counter : IResilientMessage
    {
        public int Count { get; set; }
        public Guid ResilientKey { get; set; } = Guid.Parse("e3b4482b-3871-4127-8f1e-135f7a9d7ec8");
    }
}
