namespace Flaminco.Migration.Abstractions
{
    public interface IMigrationService
    {
        void Migrate<TScriptScanner>();
    }
}
