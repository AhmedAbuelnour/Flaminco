using DbUp;
using DbUp.Engine;
using Flaminco.Migration.Abstractions;

namespace Flaminco.Migration.Implementations
{
    public class DbUpMigrationService(string connectionString) : IMigrationService
    {
        /// <summary>
        /// Executes the database migration scripts embedded in the specified assembly.
        /// </summary>
        /// <typeparam name="TScriptScanner">The type used to locate the assembly containing the migration scripts.</typeparam>
        public void Migrate<TScriptScanner>() where TScriptScanner : class
        {
            UpgradeEngine upgrader = DeployChanges.To.SqlDatabase(connectionString)
                                                     .WithScriptsEmbeddedInAssembly(typeof(TScriptScanner).Assembly)
                                                     .LogToConsole()
                                                     .Build();

            DatabaseUpgradeResult result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                throw result.Error;
            }
        }
    }
}
