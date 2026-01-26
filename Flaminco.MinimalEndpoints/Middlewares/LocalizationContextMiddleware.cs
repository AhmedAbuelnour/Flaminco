using Flaminco.MinimalEndpoints.Abstractions;
using Flaminco.MinimalEndpoints.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.MinimalEndpoints.Middlewares
{
    /// <summary>
    /// Middleware that sets the LocalizationContext for the current request using the registered IStringLocalizer.
    /// </summary>
    public class LocalizationContextMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // Set the current localizer in the async context
            LocalizationContext.SetCurrent(context.RequestServices.GetService<IPropertyAwareTextLocator>());

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
}
