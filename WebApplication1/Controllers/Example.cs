using Flaminco.CacheKeys;
using Flaminco.RediPolly.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;
using WebApplication1.Publishers;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Example(IPublisherLocator _locator)
    {
        [HttpGet]
        [Route("publish")]
        public async Task Publish(HybridCache hybridCache)
        {
            string dummyValue = await hybridCache.GetOrCreateAsync<string>(new CacheKey
            {
                Region = "Lookup",
                Key = "Categories",
                Tags = ["v1", "api"]
            }, async (x) =>
            {
                Console.WriteLine("Get from data store");
                return "dummy value";
            }, tags: ["tag1", "tag2"]);

            // incase you need to remove all cached items by tag
            await hybridCache.RemoveTagAsync("v1");



            Console.WriteLine(vau);

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
