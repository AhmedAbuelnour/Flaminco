using DbUp;
using DbUp.Helpers;
using Flaminco.Migration.SqlServer.Abstractions;
using Flaminco.Migration.SqlServer.Extensions;
using Flaminco.Migration.SqlServer.Options;

namespace Flaminco.Migration.SqlServer.Implementations;

internal class DbUpMigrationService(MigrationOptions migrationOptions) : IMigrationService
{

    /// <inheritdoc />
    public void Migrate<TScriptScanner>() where TScriptScanner : class
    {
        EnsureDatabase.For.SqlDatabase(migrationOptions.ConnectionString);

        MigrateJournalInternal<TScriptScanner>();

        MigrateAlwaysExecuteInternal<TScriptScanner>();
    }

    internal void MigrateJournalInternal<TScriptScanner>()
    {
        var upgradeEngineBuilder = DeployChanges.To.SqlDatabase(migrationOptions.ConnectionString)
            .WithTransaction()
            .LogToConsole();

        upgradeEngineBuilder = migrationOptions.Directories?.Any() ?? false
            ? upgradeEngineBuilder.WithScriptsEmbeddedInDirectories(typeof(TScriptScanner).Assembly,
                migrationOptions.Directories, s => !IsAlwaysExecute(s))
            : upgradeEngineBuilder.WithScriptsEmbeddedInAssembly(typeof(TScriptScanner).Assembly,
                s => !IsAlwaysExecute(s));

        upgradeEngineBuilder.Build().RunUpgrader();
    }


    internal void MigrateAlwaysExecuteInternal<TScriptScanner>()
    {
        var alwaysExecuteUpgradeEngineBuilder = DeployChanges.To.SqlDatabase(migrationOptions.ConnectionString)
            .JournalTo(new NullJournal())
            .WithTransaction()
            .LogToConsole();

        alwaysExecuteUpgradeEngineBuilder = migrationOptions.Directories?.Any() ?? false
            ? alwaysExecuteUpgradeEngineBuilder.WithScriptsEmbeddedInDirectories(typeof(TScriptScanner).Assembly,
                migrationOptions.Directories, IsAlwaysExecute)
            : alwaysExecuteUpgradeEngineBuilder.WithScriptsEmbeddedInAssembly(typeof(TScriptScanner).Assembly,
                IsAlwaysExecute);


        alwaysExecuteUpgradeEngineBuilder.Build().RunUpgrader();
    }

    private bool IsAlwaysExecute(string scriptName)
    {
        foreach (string directory in migrationOptions.AlwaysExecuteDirectories ?? [])
            if (scriptName.StartsWith(directory, StringComparison.OrdinalIgnoreCase))
                return true;

        return false;
    }
}