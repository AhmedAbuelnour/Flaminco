using Flaminco.MinimalEndpoints.Exceptions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;

namespace Flaminco.MinimalEndpoints.Implementations
{
    internal sealed class DefaultTextLocator(IOptions<LocalizationOptions> options) : IStringLocalizer
    {
        // Static cache for storing localization data per culture.
        private static readonly ConcurrentDictionary<string, JsonElement> _localizationCache = new();

        public LocalizedString this[string name]
        {
            get
            {
                string? value = GetValueFromJson(name, CultureInfo.CurrentUICulture.Name);
                return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                string? localizedValue = GetValueFromJson(name, CultureInfo.CurrentUICulture.Name);
                string value = string.Format(localizedValue ?? name, arguments);
                return new LocalizedString(name, value, resourceNotFound: localizedValue == null);
            }
        }

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
