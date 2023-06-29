using Flaminco.ManualMapper.Abstractions;
using Flaminco.Pipeline.Abstractions;
using Flaminco.ProDownloader.HttpClients;
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

            DownloaderClient fileDownloader = new DownloaderClient(httpClientFactory);

            await fileDownloader.DownloadAsync(
                url: "https://raw.githubusercontent.com/AhmedAbuelnour/MBs/master/4MB.txt",
                downloadPath: @"D:\Downloads",
                CurrentProgress: (e) =>
                {
                    Console.WriteLine($"Download Speed: {e.DownloadSpeed}");
                    Console.WriteLine($"Percentage: {e.CurrentPercentage}");
                    Console.WriteLine($"Downloaded Progress: {e.DownloadedProgress}");
                },
                chunkNumbers: 2);


            return Ok();
        }


    }
}