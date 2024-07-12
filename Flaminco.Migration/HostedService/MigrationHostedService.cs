using Flaminco.Migration.Abstractions;
using Microsoft.Extensions.Hosting;

namespace Flaminco.Migration.HostedService
{
    /// <summary>
    /// Using Hosted Service, to block the application to accept any requests until the migration successfully done,
    /// And if the migration didn't complete successfully it will not let the app to work.
    /// </summary>
    internal class MigrationHostedService<TScriptScanner> : IHostedService where TScriptScanner : class
    {
        private readonly IMigrationService _migrationService;
        public MigrationHostedService(IMigrationService migrationService)
        {
            _migrationService = migrationService;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _migrationService.Migrate<TScriptScanner>();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
