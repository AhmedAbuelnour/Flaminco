using DbUp;
using DbUp.Builder;
using DbUp.Helpers;
using DbUp.Oracle;
using Flaminco.Migration.Oracle.Abstractions;
using Flaminco.Migration.Oracle.Extensions;
using Flaminco.Migration.Oracle.Options;

namespace Flaminco.Migration.Oracle.Implementations
{
    internal class DbUpMigrationService(MigrationOptions _migrationOptions) : IMigrationService
    {

        /// <inheritdoc/>
        public void Migrate<TScriptScanner>() where TScriptScanner : class
        {
            EnsureDatabase.For.ForOracleDatabase(_migrationOptions.ConnectionString);

            MigrateJournalInternal<TScriptScanner>();

            MigrateAlwaysExecuteInternal<TScriptScanner>();
        }

        internal void MigrateJournalInternal<TScriptScanner>()
        {
            UpgradeEngineBuilder upgradeEngineBuilder = DeployChanges.To.OracleDatabaseWithDefaultDelimiter(_migrationOptions.ConnectionString)
                                                                        .WithTransaction()
                                                                        .LogToConsole();

            upgradeEngineBuilder = _migrationOptions.Directories?.Any() ?? false
                ? upgradeEngineBuilder.WithScriptsEmbeddedInDirectories(typeof(TScriptScanner).Assembly, _migrationOptions.Directories, s => !IsAlwaysExecute(s))
                : upgradeEngineBuilder.WithScriptsEmbeddedInAssembly(typeof(TScriptScanner).Assembly, s => !IsAlwaysExecute(s));

            upgradeEngineBuilder.Build().RunUpgrader();
        }


        internal void MigrateAlwaysExecuteInternal<TScriptScanner>()
        {
            UpgradeEngineBuilder alwaysExecuteUpgradeEngineBuilder = DeployChanges.To.OracleDatabaseWithDefaultDelimiter(_migrationOptions.ConnectionString)
                                                                                     .JournalTo(new NullJournal())
                                                                                     .WithTransaction()
                                                                                     .LogToConsole();

            alwaysExecuteUpgradeEngineBuilder = _migrationOptions.Directories?.Any() ?? false
                ? alwaysExecuteUpgradeEngineBuilder.WithScriptsEmbeddedInDirectories(typeof(TScriptScanner).Assembly, _migrationOptions.Directories, IsAlwaysExecute)
                : alwaysExecuteUpgradeEngineBuilder.WithScriptsEmbeddedInAssembly(typeof(TScriptScanner).Assembly, IsAlwaysExecute);


            alwaysExecuteUpgradeEngineBuilder.Build().RunUpgrader();
        }

        private bool IsAlwaysExecute(string scriptName)
        {
            foreach (string directory in _migrationOptions.AlwaysExecuteDirectories ?? [])
            {
                if (scriptName.StartsWith(directory, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
