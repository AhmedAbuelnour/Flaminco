using DbUp;
using DbUp.Builder;
using DbUp.Helpers;
using Flaminco.Migration.Abstractions;
using Flaminco.Migration.Extensions;
using Flaminco.Migration.Options;

namespace Flaminco.Migration.Implementations
{
    internal class DbUpMigrationService : IMigrationService
    {
        private readonly MigrationOptions _migrationOptions;
        public DbUpMigrationService(MigrationOptions migrationOptions) => _migrationOptions = migrationOptions;

        /// <inheritdoc/>
        public void Migrate<TScriptScanner>() where TScriptScanner : class
        {
            EnsureDatabase.For.SqlDatabase(_migrationOptions.ConnectionString);

            MigrateJournalInternal<TScriptScanner>();

            MigrateAlwaysExecuteInternal<TScriptScanner>();
        }

        internal void MigrateJournalInternal<TScriptScanner>()
        {
            UpgradeEngineBuilder upgradeEngineBuilder = DeployChanges.To.SqlDatabase(_migrationOptions.ConnectionString)
                                                                        .WithTransaction()
                                                                        .LogToConsole();

            upgradeEngineBuilder = _migrationOptions.Directories?.Any() ?? false
                ? upgradeEngineBuilder.WithScriptsEmbeddedInDirectories(typeof(TScriptScanner).Assembly, _migrationOptions.Directories, s => !IsAlwaysExecute(s))
                : upgradeEngineBuilder.WithScriptsEmbeddedInAssembly(typeof(TScriptScanner).Assembly, s => !IsAlwaysExecute(s));

            upgradeEngineBuilder.Build().RunUpgrader();
        }


        internal void MigrateAlwaysExecuteInternal<TScriptScanner>()
        {
            UpgradeEngineBuilder alwaysExecuteUpgradeEngineBuilder = DeployChanges.To.SqlDatabase(_migrationOptions.ConnectionString)
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
            foreach (string directory in _migrationOptions.AlwaysExecuteDirectories ?? Array.Empty<string>())
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
