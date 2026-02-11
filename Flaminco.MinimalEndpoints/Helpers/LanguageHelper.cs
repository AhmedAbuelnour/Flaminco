namespace Flaminco.MinimalEndpoints.Helpers;

/// <summary>
/// Helper class for language code normalization.
/// Normalizes language codes to base language (e.g., "ar-eg" -> "ar", "en-US" -> "en").
/// </summary>
internal static class LanguageHelper
{
    /// <summary>
    /// Normalizes a language code to its base language.
    /// Examples: "ar-eg" -> "ar", "en-US" -> "en", "ar" -> "ar"
    /// </summary>
    /// <param name="languageCode">The language code to normalize (e.g., "ar-eg", "en-US", "ar")</param>
    /// <returns>The normalized base language code (e.g., "ar" or "en")</returns>
    public static string NormalizeLanguageCode(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return "en"; // Default fallback
        }

        // Remove any whitespace
        languageCode = languageCode.Trim();

        // Extract base language (part before hyphen)
        int hyphenIndex = languageCode.IndexOf('-');
        if (hyphenIndex > 0)
        {
            string baseLanguage = languageCode.Substring(0, hyphenIndex).ToLowerInvariant();
            return baseLanguage;
        }

        // If no hyphen, return the language code in lowercase
        return languageCode.ToLowerInvariant();
    }
}

