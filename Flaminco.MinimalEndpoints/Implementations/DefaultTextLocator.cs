using Flaminco.MinimalEndpoints.Exceptions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;

namespace Flaminco.MinimalEndpoints.Implementations
{
    /// <summary>
    /// Provides a default implementation of <see cref="IStringLocalizer"/> for retrieving localized strings
    /// from JSON files based on the current UI culture.
    /// </summary>
    internal sealed class DefaultTextLocator(IOptions<LocalizationOptions> options) : IStringLocalizer
    {
        // Static cache for storing localization data per culture.
        private static readonly ConcurrentDictionary<string, JsonElement> _localizationCache = new();

        /// <summary>
        /// Gets the localized string for the specified key.
        /// </summary>
        /// <param name="name">The key of the string to localize.</param>
        /// <returns>A <see cref="LocalizedString"/> containing the localized value or the key if not found.</returns>
        public LocalizedString this[string name]
        {
            get
            {
                string? value = GetValueFromJson(name, CultureInfo.CurrentUICulture.Name);
                return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
            }
        }

        /// <summary>
        /// Gets the localized string for the specified key and formats it with the provided arguments.
        /// </summary>
        /// <param name="name">The key of the string to localize.</param>
        /// <param name="arguments">The arguments to format the localized string.</param>
        /// <returns>A <see cref="LocalizedString"/> containing the formatted localized value or the key if not found.</returns>
        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                string? localizedValue = GetValueFromJson(name, CultureInfo.CurrentUICulture.Name);
                string value = string.Format(localizedValue ?? name, arguments);
                return new LocalizedString(name, value, resourceNotFound: localizedValue == null);
            }
        }

        /// <summary>
        /// Retrieves the localized value for the specified key and culture from the JSON file.
        /// </summary>
        /// <param name="key">The key of the string to retrieve.</param>
        /// <param name="culture">The culture for which to retrieve the localized string.</param>
        /// <returns>The localized string value, or null if not found.</returns>
        /// <exception cref="LocalizationNotFoundException">Thrown if the JSON file for the specified culture is not found.</exception>
        private string? GetValueFromJson(string key, string culture)
        {
            // Try to get the cached JSON object for the requested culture.
            if (!_localizationCache.TryGetValue(culture, out var jsonObject))
            {
                string jsonFilePath = Path.Combine(options.Value.ResourcesPath, $"{culture}.json");

                if (!File.Exists(jsonFilePath))
                {
                    throw new LocalizationNotFoundException(jsonFilePath);
                }

                // Asynchronously read and parse the JSON file.
                string jsonContent = File.ReadAllText(jsonFilePath);
                using var document = JsonDocument.Parse(jsonContent);
                jsonObject = document.RootElement.Clone();

                // Add the parsed JSON to the static cache.
                _localizationCache[culture] = jsonObject;
            }

            return jsonObject.TryGetProperty(key, out var jsonValue) ? jsonValue.GetString() : null;
        }

        /// <summary>
        /// Retrieves all localized strings for the current UI culture.
        /// </summary>
        /// <param name="includeParentCultures">Indicates whether to include strings from parent cultures.</param>
        /// <returns>An enumerable of <see cref="LocalizedString"/> containing all localized strings.</returns>
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            string culture = CultureInfo.CurrentUICulture.Name;

            if (!_localizationCache.TryGetValue(culture, out var jsonObject))
            {
                string jsonFilePath = Path.Combine(options.Value.ResourcesPath, $"{culture}.json");

                if (!File.Exists(jsonFilePath))
                {
                    return [];
                }

                string jsonContent = File.ReadAllText(jsonFilePath);
                using var document = JsonDocument.Parse(jsonContent);
                jsonObject = document.RootElement.Clone();

                _localizationCache[culture] = jsonObject;
            }

            return jsonObject.EnumerateObject()
                             .Select(prop => new LocalizedString(prop.Name, prop.Value.GetString() ?? prop.Name, resourceNotFound: false));
        }
    }

}
