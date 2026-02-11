using Flaminco.MinimalEndpoints.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Flaminco.MinimalEndpoints.Middlewares;

/// <summary>
/// Middleware that sets the LocalizationContext for the current request using the registered IStringLocalizer.
/// </summary>
internal class LocalizationContextMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Set the current localizer in the async context
        LocalizationContext.SetCurrent(context.RequestServices.GetService<IStringLocalizer>());

        try
        {
            await next(context);
        }
        finally
        {
            // Clean up the context after the request
            LocalizationContext.Clear();
        }
    }
}
