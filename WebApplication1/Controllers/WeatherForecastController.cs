using Flaminco.RedisChannels.Subscribers;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ChannelPublisher _cacheService;
        public WeatherForecastController(ChannelPublisher cacheService)
        {
            _cacheService = cacheService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<string?> Get()
        {
            await _cacheService.PublishAsync(new Counter
            {
                Count = 5
            });

            return "HEHEHE";
        }
    }


    public class Counter
    {
        public int Count { get; set; }
    }
}
