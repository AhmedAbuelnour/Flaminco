using DbUp;
using DbUp.Engine;
using Flaminco.Migration.Abstractions;

namespace Flaminco.Migration.Implementations
{
    public class DbUpMigrationService(string connectionString) : IMigrationService
    {
        public void Migrate<TScriptScanner>()
        {
            UpgradeEngine upgrader = DeployChanges.To.SqlDatabase(connectionString)
                                                     .WithScriptsEmbeddedInAssembly(typeof(TScriptScanner).Assembly)
                                                     .LogToConsole()
                                                     .Build();

            DatabaseUpgradeResult result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                throw new InvalidOperationException("Database migration failed", result.Error);
            }
        }
    }
}
