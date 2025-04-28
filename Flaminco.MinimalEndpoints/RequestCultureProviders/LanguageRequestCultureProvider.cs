using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace Flaminco.MinimalEndpoints.RequestCultureProviders
{
    /// <summary>
    /// Provides culture information for HTTP requests based on language headers.
    /// </summary>
    /// <remarks>
    /// This provider examines multiple HTTP headers in order of priority:
    /// 1. X-Culture header
    /// 2. Language header
    /// 3. Accept-Language header
    /// 4. Default language parameter
    /// 5. Fallback to "en" if none of the above are available
    /// </remarks>
    /// <param name="defaultLanguage">The default language to use if no language headers are present</param>
    internal class LanguageRequestCultureProvider(string? defaultLanguage) : RequestCultureProvider
    {
        /// <summary>
        /// Determines the culture for the current HTTP request based on header values.
        /// </summary>
        /// <param name="httpContext">The current HTTP context</param>
        /// <returns>A task that represents the asynchronous operation, containing the determined provider culture result or null</returns>
        public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
        {
            // Look for 'X-Culture' header or fall back to 'Accept-Language'
            string cultureHeader = httpContext.Request.Headers["X-Culture"].FirstOrDefault()
                                  ?? httpContext.Request.Headers["Language"].FirstOrDefault()
                                  ?? httpContext.Request.Headers.AcceptLanguage.FirstOrDefault()
                                  ?? defaultLanguage
                                  ?? "en";

            // Use the first trimmed language value.
            string culture = cultureHeader.Split(',')
                                          .Select(c => c.Trim())
                                          .FirstOrDefault() ?? defaultLanguage ?? "en";

            return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(culture, culture));
        }
    }
}
