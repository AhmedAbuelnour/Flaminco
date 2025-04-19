namespace Flaminco.MinimalEndpoints.Exceptions
{
    internal class LocalizationNotFoundException(string baseName) : Exception($"{baseName} Localization file not found")
    {
    }
}
