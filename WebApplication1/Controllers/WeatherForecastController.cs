using Flaminco.ManualMapper;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Mappers;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IManualMapper<int> _mapper;
        public WeatherForecastController(IManualMapper<int> mapper)
        {
            _mapper = mapper;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async ValueTask<int> Get()
        {
            return await _mapper.Map(new SimpleMapProfile("Ahmed"), opts =>
            {
                opts.Arguments["ReturnValue"] = 5;
            });
        }
    }
}