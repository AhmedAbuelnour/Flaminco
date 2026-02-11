using Flaminco.MinimalEndpoints.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace Flaminco.MinimalEndpoints.RequestCultureProviders
{
    /// <summary>
    /// Provides culture information for HTTP requests based on language headers.
    /// Automatically normalizes language codes to base language (e.g., "ar-eg" -> "ar").
    /// </summary>
    /// <remarks>
    /// This provider examines multiple HTTP headers in order of priority:
    /// 1. X-Culture header
    /// 2. Language header
    /// 3. Accept-Language header
    /// 4. Default language parameter
    /// 5. Fallback to "en" if none of the above are available
    /// 
    /// The language code is normalized to its base language (e.g., "ar-eg" -> "ar", "en-US" -> "en").
    /// </remarks>
    /// <param name="defaultLanguage">The default language to use if no language headers are present</param>
    internal class LanguageRequestCultureProvider(string? defaultLanguage) : RequestCultureProvider
    {
        /// <summary>
        /// Determines the culture for the current HTTP request based on header values.
        /// Automatically normalizes the language code to base language.
        /// </summary>
        /// <param name="httpContext">The current HTTP context</param>
        /// <returns>A task that represents the asynchronous operation, containing the determined provider culture result or null</returns>
        public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
        {
            // Look for 'X-Culture' header or fall back to 'Accept-Language'
            string? cultureHeader = httpContext.Request.Headers["X-Culture"].FirstOrDefault()
                                  ?? httpContext.Request.Headers["Language"].FirstOrDefault()
                                  ?? httpContext.Request.Headers.AcceptLanguage.FirstOrDefault()
                                  ?? defaultLanguage;

            // Use the first trimmed language value.
            string? rawCulture = cultureHeader?.Split(',')
                                          .Select(c => c.Trim())
                                          .FirstOrDefault() ?? defaultLanguage;

            // Normalize the language code to base language (e.g., "ar-eg" -> "ar")
            string normalizedCulture = LanguageHelper.NormalizeLanguageCode(rawCulture);

            return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(normalizedCulture, normalizedCulture));
        }
    }
}
