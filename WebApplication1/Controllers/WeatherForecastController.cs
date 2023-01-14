using Flaminco.ManualMapper.Abstractions;
using Flaminco.Pipeline.Abstractions;
using Flaminco.StateMachine.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApplication1.Mappers;
using WebApplication1.StateMachines;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IPipeline _pipeline;
        private readonly IManualMapper _manualMapper;
        private readonly IStateContext _stateContext;
        public WeatherForecastController(IPipeline pipeline, IManualMapper manualMapper, IStateContext stateContext)
        {
            _pipeline = pipeline;
            _manualMapper = manualMapper;
            _stateContext = stateContext;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<string> Get()
        {

            await _manualMapper.Map(new SimpleMapProfile("AAA"));

            var sharedValue = new SharedValue();

            await _stateContext.Execute(
                new FirstState(),
                sharedValue,
                onTransition: (currentState) =>
                {
                    Debug.WriteLine($"CurrentState: {currentState.Name}");
                },
                onComplete: (states) =>
                {
                    Debug.WriteLine(states.Count());
                });

            return $"Value: {sharedValue.Value}";
        }
    }
}