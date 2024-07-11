namespace Flaminco.Migration.Options
{
    /// <summary>
    /// Represents the configuration options for database migration.
    /// </summary>
    public class MigrationOptions
    {
        /// <summary>
        /// Gets or sets the connection string used to connect to the database.
        /// </summary>
        public string ConnectionString { get; set; } = default!;

        /// <summary>
        /// Gets or sets the directories containing the migration scripts.
        /// </summary>
        public string[]? Directories { get; set; }

        /// <summary>
        /// Validates the migration options.
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new ArgumentException("ConnectionString must be set in the migration options.");
            }
        }
    }
}
