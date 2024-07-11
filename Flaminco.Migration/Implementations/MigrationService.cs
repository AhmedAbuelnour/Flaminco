using DbUp;
using DbUp.Builder;
using DbUp.Engine;
using Flaminco.Migration.Abstractions;
using Flaminco.Migration.Extensions;
using Flaminco.Migration.Options;

namespace Flaminco.Migration.Implementations
{
    internal class DbUpMigrationService(MigrationOptions _migrationOptions) : IMigrationService
    {
        /// <inheritdoc/>
        public void Migrate<TScriptScanner>() where TScriptScanner : class
        {
            EnsureDatabase.For.SqlDatabase(_migrationOptions.ConnectionString);

            UpgradeEngineBuilder upgradeEngineBuilder = DeployChanges.To.SqlDatabase(_migrationOptions.ConnectionString).LogToConsole();

            upgradeEngineBuilder = _migrationOptions.Directories?.Any() ?? false
                ? upgradeEngineBuilder.WithScriptsEmbeddedInDirectories(typeof(TScriptScanner).Assembly, _migrationOptions.Directories)
                : upgradeEngineBuilder.WithScriptsEmbeddedInAssembly(typeof(TScriptScanner).Assembly);


            UpgradeEngine upgrader = upgradeEngineBuilder.Build();

            if (upgrader.IsUpgradeRequired())
            {
                DatabaseUpgradeResult databaseUpgradeResult = upgrader.PerformUpgrade();

                if (!databaseUpgradeResult.Successful)
                {
                    throw databaseUpgradeResult.Error;
                }
            }
        }

    }
}
