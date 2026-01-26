using Microsoft.Extensions.Localization;

namespace Flaminco.MinimalEndpoints.Abstractions
{
    /// <summary>
    /// Extends IStringLocalizer to support property-based localization file lookups.
    /// </summary>
    public interface IPropertyAwareTextLocator : IStringLocalizer
    {
        /// <summary>
        /// Gets the localized string for the specified key using property-specific JSON files.
        /// </summary>
        /// <param name="key">The key of the string to localize.</param>
        /// <param name="propertyName">The property name to use for finding the appropriate JSON file.</param>
        /// <param name="culture">The culture for which to retrieve the localized string. If null, uses current UI culture.</param>
        /// <returns>A LocalizedString containing the localized value or the key if not found.</returns>
        LocalizedString GetByProperty(string key, string propertyName, string? culture = null);
    }
}
