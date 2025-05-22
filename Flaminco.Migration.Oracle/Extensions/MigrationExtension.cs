using DbUp;
using DbUp.Builder;
using DbUp.Engine;
using Flaminco.Migration.Oracle.Abstractions;
using Flaminco.Migration.Oracle.HostedService;
using Flaminco.Migration.Oracle.Implementations;
using Flaminco.Migration.Oracle.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;
using System.Reflection;

namespace Flaminco.Migration.Oracle.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring and adding database migration services.
    /// </summary>
    public static class MigrationExtension
    {
        /// <summary>
        /// Adds the migration service to the dependency injection container using the specified configuration section.
        /// </summary>
        public static IServiceCollection AddMigration<TScriptScanner>(this IServiceCollection services, IConfiguration configuration, string sectionName = "Migration") where TScriptScanner : class
        {
            MigrationOptions migrationOptions = new();

            configuration.GetSection(sectionName).Bind(migrationOptions);

            migrationOptions.Validate();

            return AddMigrationInternal<TScriptScanner>(services, migrationOptions);
        }

        /// <summary>
        /// Adds the migration service to the dependency injection container using the specified options.
        /// </summary>
        public static IServiceCollection AddMigration<TScriptScanner>(this IServiceCollection services, Action<MigrationOptions> configureOptions) where TScriptScanner : class
        {
            MigrationOptions migrationOptions = new();

            configureOptions(migrationOptions);

            migrationOptions.Validate();

            return AddMigrationInternal<TScriptScanner>(services, migrationOptions);
        }

        /// <summary>
        /// Adds the migration service to the dependency injection container using the provided migration options.
        /// </summary>
        internal static IServiceCollection AddMigrationInternal<TScriptScanner>(IServiceCollection services, MigrationOptions migrationOptions) where TScriptScanner : class
        {
            services.AddSingleton<IMigrationService>(new DbUpMigrationService(migrationOptions));

            services.AddHostedService<MigrationHostedService<TScriptScanner>>();

            return services;
        }


        /// <summary>
        /// Configures the UpgradeEngineBuilder to use scripts embedded in the specified directories within the assembly.
        /// </summary>
        internal static UpgradeEngineBuilder WithScriptsEmbeddedInDirectories(this UpgradeEngineBuilder builder, Assembly assembly, string[] directories, Func<string, bool> filter)
        {
            string[] files = assembly.GetManifestResourceNames();

            foreach (string directory in directories)
            {
                builder.WithScripts(files.Where(name => name.StartsWith(directory, StringComparison.InvariantCultureIgnoreCase) && name.EndsWith(".sql"))
                                         .Where(filter)
                                         .OrderBy(name => name)
                                         .Select(name =>
                                          {
                                              using var stream = assembly.GetManifestResourceStream(name)!;
                                              using var reader = new StreamReader(stream);
                                              var scriptContent = reader.ReadToEnd();
                                              return new SqlScript(name, scriptContent);
                                          }));
            }

            return builder.WithFilter(new DirectoryScriptFilter(directories));
        }

        /// <summary>
        /// Configures the UpgradeEngineBuilder to use scripts embedded in the specified directories within the assembly.
        /// </summary>
        internal static UpgradeEngineBuilder WithScriptsEmbeddedInDirectories(this UpgradeEngineBuilder builder, Assembly assembly, string[] directories)
        {
            string[] files = assembly.GetManifestResourceNames();

            foreach (string directory in directories)
            {
                builder.WithScripts(files.Where(name => name.StartsWith(directory) && name.EndsWith(".sql"))
                                         .OrderBy(name => name)
                                         .Select(name =>
                                         {
                                             using var stream = assembly.GetManifestResourceStream(name)!;
                                             using var reader = new StreamReader(stream);
                                             var scriptContent = reader.ReadToEnd();
                                             return new SqlScript(name, scriptContent);
                                         }));
            }

            return builder.WithFilter(new DirectoryScriptFilter(directories));
        }



        /// <summary>
        /// Run the upgrader to migrate the scripts.
        /// </summary>
        internal static void RunUpgrader(this UpgradeEngine upgradeEngine)
        {
            if (upgradeEngine.IsUpgradeRequired())
            {
                DatabaseUpgradeResult databaseUpgradeResult = upgradeEngine.PerformUpgrade();

                if (!databaseUpgradeResult.Successful)
                {
                    throw databaseUpgradeResult.Error;
                }
            }
        }

        internal static void ForOracleDatabase(this SupportedDatabasesForEnsureDatabase _, string connectionString)
        {
            try
            {
                using (var connection = new OracleConnection(connectionString))
                {
                    connection.Open();
                    // If the connection opens successfully, the database exists
                    Console.WriteLine("Oracle database connection established.");
                }
            }
            catch
            {
                // Handle any exception that might indicate the database does not exist or is not accessible
                Console.WriteLine("Failed to connect to the Oracle database. Please verify that the database exists and the connection string is correct.");
                throw;
            }
        }
    }
}
