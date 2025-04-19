using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace Flaminco.MinimalEndpoints.RequestCultureProviders
{
    internal class LanguageRequestCultureProvider(string? defaultLanguage) : RequestCultureProvider
    {
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
