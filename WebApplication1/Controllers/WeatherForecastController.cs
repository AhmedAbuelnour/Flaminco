using Flaminco.RedisChannels.Abstractions;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Publishers;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IPublisherLocator _cacheService;
        public WeatherForecastController(IPublisherLocator cacheService)
        {
            _cacheService = cacheService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<string?> Get()
        {
            if (_cacheService.GetPublisher<TESTRedisPublisher>() is TESTRedisPublisher redisPublisher)
            {
                await redisPublisher.PublishAsync(new Counter
                {
                    Count = 5
                });
            }

            return "HEHEHE";
        }
    }


    public class Counter
    {
        public int Count { get; set; }
    }
}
