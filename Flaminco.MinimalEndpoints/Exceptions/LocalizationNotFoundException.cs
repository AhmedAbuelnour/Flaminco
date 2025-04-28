namespace Flaminco.MinimalEndpoints.Exceptions
{
    /// <summary>
    /// Exception thrown when a localization file is not found for the specified base name.
    /// </summary>
    internal class LocalizationNotFoundException(string baseName) : Exception($"{baseName} Localization file not found")
    {
    }
}
