using Flaminco.MinimalEndpoints.Helpers;
using Microsoft.Extensions.Localization;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;

namespace Flaminco.MinimalEndpoints.Implementations;

/// <summary>
/// A native IStringLocalizer implementation that reads from JSON files.
/// Supports nested JSON structures using dot-notation (e.g., "Errors.USER_NOT_FOUND").
/// Automatically normalizes language codes (e.g., "ar-eg" -> "ar") when loading JSON files.
/// </summary>
internal sealed class JsonStringLocalizer : IStringLocalizer
{
    private readonly string _resourcesPath;
    private static readonly ConcurrentDictionary<string, JsonElement> _cache = new();

    public JsonStringLocalizer(string resourcesPath)
    {
        _resourcesPath = resourcesPath;
    }

    /// <inheritdoc/>
    public LocalizedString this[string name]
    {
        get
        {
            string culture = LanguageHelper.NormalizeLanguageCode(CultureInfo.CurrentUICulture.Name);
            string? value = GetValue(name, culture);
            return new LocalizedString(name, value ?? name, resourceNotFound: value is null);
        }
    }

    /// <inheritdoc/>
    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            string culture = LanguageHelper.NormalizeLanguageCode(CultureInfo.CurrentUICulture.Name);
            string? value = GetValue(name, culture);
            string formatted = value is not null 
                ? string.Format(value, arguments) 
                : string.Format(name, arguments);
            return new LocalizedString(name, formatted, resourceNotFound: value is null);
        }
    }

    /// <inheritdoc/>
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        string culture = LanguageHelper.NormalizeLanguageCode(CultureInfo.CurrentUICulture.Name);
        var json = LoadJson(culture);
        
        if (json is null)
            return [];

        return FlattenJsonElement(json.Value, string.Empty);
    }

    private string? GetValue(string key, string culture)
    {
        var json = LoadJson(culture);
        
        if (json is null)
            return null;

        // Support dot-notation for nested keys (e.g., "Errors.USER_NOT_FOUND")
        return GetNestedValue(json.Value, key);
    }

    /// <summary>
    /// Gets a value from a nested JSON structure using dot-notation.
    /// For example, "Errors.USER_NOT_FOUND" will navigate to { "Errors": { "USER_NOT_FOUND": "..." } }
    /// </summary>
    private static string? GetNestedValue(JsonElement element, string key)
    {
        // First try direct property access (flat structure)
        if (element.TryGetProperty(key, out var directValue))
        {
            return directValue.ValueKind == JsonValueKind.String ? directValue.GetString() : null;
        }

        // Then try dot-notation navigation (nested structure)
        string[] parts = key.Split('.');
        JsonElement current = element;

        foreach (string part in parts)
        {
            if (current.ValueKind != JsonValueKind.Object)
                return null;

            if (!current.TryGetProperty(part, out current))
                return null;
        }

        return current.ValueKind == JsonValueKind.String ? current.GetString() : null;
    }

    /// <summary>
    /// Flattens a nested JSON structure into dot-notation keys.
    /// </summary>
    private static IEnumerable<LocalizedString> FlattenJsonElement(JsonElement element, string prefix)
    {
        if (element.ValueKind != JsonValueKind.Object)
            yield break;

        foreach (var property in element.EnumerateObject())
        {
            string key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";

            if (property.Value.ValueKind == JsonValueKind.String)
            {
                yield return new LocalizedString(key, property.Value.GetString() ?? key, resourceNotFound: false);
            }
            else if (property.Value.ValueKind == JsonValueKind.Object)
            {
                foreach (var nested in FlattenJsonElement(property.Value, key))
                {
                    yield return nested;
                }
            }
        }
    }

    private JsonElement? LoadJson(string culture)
    {
        if (_cache.TryGetValue(culture, out var cached))
            return cached;

        string filePath = Path.Combine(_resourcesPath, $"{culture}.json");

        if (!File.Exists(filePath))
            return null;

        string content = File.ReadAllText(filePath);
        using var doc = JsonDocument.Parse(content);
        var element = doc.RootElement.Clone();

        _cache[culture] = element;
        return element;
    }

    /// <summary>
    /// Clears the localization cache. Useful for testing or dynamic reload scenarios.
    /// </summary>
    public static void ClearCache() => _cache.Clear();
}
