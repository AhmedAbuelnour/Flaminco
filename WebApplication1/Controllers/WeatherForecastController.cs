using Flaminco.ManualMapper.Abstractions;
using Flaminco.Pipeline.Abstractions;
using Flaminco.ProDownloader.Utilities;
using Flaminco.StateMachine.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IPipeline _pipeline;
        private readonly IManualMapper _manualMapper;
        private readonly IStateContext _stateContext;
        private readonly IHttpClientFactory httpClientFactory;
        public WeatherForecastController(IPipeline pipeline, IManualMapper manualMapper, IStateContext stateContext, IHttpClientFactory httpClientFactory)
        {
            _pipeline = pipeline;
            _manualMapper = manualMapper;
            _stateContext = stateContext;
            this.httpClientFactory = httpClientFactory;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IActionResult> Get()
        {

            FileDownloader fileDownloader = new FileDownloader(httpClientFactory);


            await fileDownloader.DownloadAsync("https://raw.githubusercontent.com/AhmedAbuelnour/MBs/master/1MB.txt", @"D:\01_01", (e) =>
            {

                Console.WriteLine($"Download Speed: {e.DownloadSpeed}");
                Console.WriteLine($"Percentage: {e.CurrentPercentage}");
                Console.WriteLine($"Downloaded Progress: {e.DownloadedProgress}");
            }, 2);

            return Ok();
        }


    }
}