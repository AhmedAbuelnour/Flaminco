using Flaminco.MinimalEndpoints.Contexts;
using Flaminco.MinimalEndpoints.Helpers;
using Microsoft.AspNetCore.Http;

namespace Flaminco.MinimalEndpoints.Middlewares
{
    internal sealed class LanguageContextMiddleware(string defaultLanguage) : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // Extract language from headers
            string? rawLanguage = context.Request.Headers["Language"].FirstOrDefault()
                                 ?? context.Request.Headers.AcceptLanguage.FirstOrDefault()
                                 ?? context.Request.Headers["X-Culture"].FirstOrDefault()
                                 ?? defaultLanguage;

            // Normalize the language code (e.g., "ar-eg" -> "ar")
            string normalizedLanguage = LanguageHelper.NormalizeLanguageCode(rawLanguage);

            LanguageContext.SetCurrent(normalizedLanguage);

            await next(context);
        }
    }
}
