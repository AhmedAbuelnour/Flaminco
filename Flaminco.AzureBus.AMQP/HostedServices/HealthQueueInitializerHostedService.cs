using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Hosting;

namespace Flaminco.AzureBus.AMQP.HostedServices
{
    internal sealed class HealthQueueInitializerHostedService(ServiceBusAdministrationClient admin) : IHostedService
    {
        public const string HealthCheckQueue = "health_check_queue";

        public async Task StartAsync(CancellationToken ct)
        {
            if (!await admin.QueueExistsAsync(HealthCheckQueue, ct))
            {
                await admin.CreateQueueAsync(HealthCheckQueue, ct);
            }
        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
