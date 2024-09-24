using Garnet;
using Microsoft.Extensions.Hosting;

namespace Flaminco.Garnet.Server.Background
{
    public abstract class GarnetServerBackgroundServiceBase : BackgroundService
    {
        public abstract string ConfigurationPath { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            GarnetServer server = new(["--config-import-path", ConfigurationPath]);

            server.Start();

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
