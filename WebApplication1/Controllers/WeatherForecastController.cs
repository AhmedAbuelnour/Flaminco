using Flaminco.ManualMapper;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Mappers;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IManualMapper _mapper;
        public WeatherForecastController(IManualMapper mapper)
        {
            _mapper = mapper;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async ValueTask<int> Get()
        {
            return await _mapper.Map<SimpleMapProfile, int>(new SimpleMapProfile("Ahmed"), opts =>
            {
                opts.Arguments["ReturnValue"] = 5;

            });
        }
    }
}