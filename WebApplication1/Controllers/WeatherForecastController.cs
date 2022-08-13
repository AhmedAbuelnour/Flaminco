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
        public int Get()
        {
            return _mapper.Map(new SimpleMapHandler(), "Ahmed", opts =>
            {
                opts.Arguments["ReturnValue"] = 5;
            }).Result;
        }
    }
}