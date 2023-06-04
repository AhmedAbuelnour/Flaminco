using Flaminco.ManualMapper.Abstractions;
using Flaminco.Pipeline.Abstractions;
using Flaminco.StateMachine.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase {
        private readonly IPipeline _pipeline;
        private readonly IManualMapper _manualMapper;
        private readonly IStateContext _stateContext;
        public WeatherForecastController(IPipeline pipeline, IManualMapper manualMapper, IStateContext stateContext) {
            _pipeline = pipeline;
            _manualMapper = manualMapper;
            _stateContext = stateContext;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IActionResult> Get() {
            var xx = BankAccount.GetResult(-1);

            return Ok(xx);
        }
    }
}