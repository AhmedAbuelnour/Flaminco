using Flaminco.Migration.Abstractions;
using Flaminco.Migration.HostedService;
using Flaminco.Migration.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.Migration.Extensions
{
    public static class MigrationExtension
    {
        public static IServiceCollection AddMigration<TScriptScanner>(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton<IMigrationService>(new DbUpMigrationService(connectionString));

            services.AddHostedService<MigrationHostedService<TScriptScanner>>();

            return services;
        }

    }
}
