namespace Flaminco.Migration.Abstractions
{
    internal interface IMigrationService
    {
        /// <summary>
        /// Executes the database migration scripts embedded in the specified assembly.
        /// </summary>
        /// <typeparam name="TScriptScanner">The type used to locate the assembly containing the migration scripts.</typeparam>
        void Migrate<TScriptScanner>() where TScriptScanner : class;
    }
}
