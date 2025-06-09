using Flaminco.MinimalEndpoints.JsonConverters;
using Flaminco.MinimalEndpoints.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Flaminco.MinimalEndpoints.Middlewares
{
    internal sealed class TimeZoneMiddleware(IOptions<TimeZoneOptions> options) : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            foreach (var headerKey in options.Value.HeaderNames)
            {
                if (context.Request.Headers.TryGetValue(headerKey, out StringValues timeZoneHeader))
                {
                    try
                    {
                        TimeZoneContext.Zone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneHeader);
                    }
                    catch (TimeZoneNotFoundException)
                    {
                        TimeZoneContext.Zone = TimeZoneInfo.Utc; // Fallback to UTC if the specified timezone is not found
                    }

                    break;
                }
            }

            await next(context);
        }
    }
}
