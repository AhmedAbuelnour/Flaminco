using DbUp.Builder;
using DbUp.Engine;
using Flaminco.Migration.SqlServer.Abstractions;
using Flaminco.Migration.SqlServer.HostedService;
using Flaminco.Migration.SqlServer.Implementations;
using Flaminco.Migration.SqlServer.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Flaminco.Migration.SqlServer.Extensions;

/// <summary>
///     Provides extension methods for configuring and adding database migration services.
/// </summary>
public static class MigrationExtension
{
    /// <summary>
    ///     Adds the migration service to the dependency injection container using the specified configuration section.
    /// </summary>
    public static IServiceCollection AddMigration<TScriptScanner>(this IServiceCollection services,
        IConfiguration configuration, string sectionName = "Migration") where TScriptScanner : class
    {
        MigrationOptions migrationOptions = new();

        configuration.GetSection(sectionName).Bind(migrationOptions);

        migrationOptions.Validate();

        return AddMigrationInternal<TScriptScanner>(services, migrationOptions);
    }

    /// <summary>
    ///     Adds the migration service to the dependency injection container using the specified options.
    /// </summary>
    public static IServiceCollection AddMigration<TScriptScanner>(this IServiceCollection services,
        Action<MigrationOptions> configureOptions) where TScriptScanner : class
    {
        MigrationOptions migrationOptions = new();

        configureOptions(migrationOptions);

        migrationOptions.Validate();

        return AddMigrationInternal<TScriptScanner>(services, migrationOptions);
    }

    /// <summary>
    ///     Adds the migration service to the dependency injection container using the provided migration options.
    /// </summary>
    internal static IServiceCollection AddMigrationInternal<TScriptScanner>(IServiceCollection services,
        MigrationOptions migrationOptions) where TScriptScanner : class
    {
        services.AddSingleton<IMigrationService>(new DbUpMigrationService(migrationOptions));

        services.AddHostedService<MigrationHostedService<TScriptScanner>>();

        return services;
    }


    /// <summary>
    ///     Configures the UpgradeEngineBuilder to use scripts embedded in the specified directories within the assembly.
    /// </summary>
    internal static UpgradeEngineBuilder WithScriptsEmbeddedInDirectories(this UpgradeEngineBuilder builder,
        Assembly assembly, string[] directories, Func<string, bool> filter)
    {
        string[] files = assembly.GetManifestResourceNames();

        foreach (string directory in directories)
            builder.WithScripts(files.Where(name => name.StartsWith(directory) && name.EndsWith(".sql"))
                .Where(filter)
                .OrderBy(name => name)
                .Select(name =>
                {
                    using var stream = assembly.GetManifestResourceStream(name)!;
                    using var reader = new StreamReader(stream);
                    var scriptContent = reader.ReadToEnd();
                    return new SqlScript(name, scriptContent);
                }));

        return builder.WithFilter(new DirectoryScriptFilter(directories));
    }

    /// <summary>
    ///     Configures the UpgradeEngineBuilder to use scripts embedded in the specified directories within the assembly.
    /// </summary>
    internal static UpgradeEngineBuilder WithScriptsEmbeddedInDirectories(this UpgradeEngineBuilder builder,
        Assembly assembly, string[] directories)
    {
        string[] files = assembly.GetManifestResourceNames();

        foreach (string directory in directories)
            builder.WithScripts(files.Where(name => name.StartsWith(directory) && name.EndsWith(".sql"))
                .OrderBy(name => name)
                .Select(name =>
                {
                    using var stream = assembly.GetManifestResourceStream(name)!;
                    using var reader = new StreamReader(stream);
                    var scriptContent = reader.ReadToEnd();
                    return new SqlScript(name, scriptContent);
                }));

        return builder.WithFilter(new DirectoryScriptFilter(directories));
    }


    /// <summary>
    ///     Run the upgrader to migrate the scripts.
    /// </summary>
    internal static void RunUpgrader(this UpgradeEngine upgradeEngine)
    {
        if (upgradeEngine.IsUpgradeRequired())
        {
            DatabaseUpgradeResult databaseUpgradeResult = upgradeEngine.PerformUpgrade();

            if (!databaseUpgradeResult.Successful) throw databaseUpgradeResult.Error;
        }
    }
}