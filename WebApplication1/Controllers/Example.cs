using Flaminco.RediPolly.Abstractions;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Publishers;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Example(IPublisherLocator _locator)
    {
        public async Task Publish()
        {
            if (_locator.GetPublisher<PublishAnyMessage>() is PublishAnyMessage redisPublisher)
            {
                await redisPublisher.PublishAsync(new Counter
                {
                    Count = 5
                });
            }
        }
    }


    public class Counter
    {
        public int Count { get; set; }
    }
}
