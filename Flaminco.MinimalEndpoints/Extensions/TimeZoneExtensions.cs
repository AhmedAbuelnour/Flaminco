using Flaminco.MinimalEndpoints.Middlewares;
using Flaminco.MinimalEndpoints.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.MinimalEndpoints.Extensions
{
    public static class TimeZoneExtensions
    {
        /// <summary>
        /// Registers TimeZoneMiddleware and configures which header names to check (in order).
        /// Note: ASP.NET Core’s HttpContext.Request.Headers is already case-insensitive by design, so TimeZone = timezone = TIMEZONE, etc. will all match.
        /// </summary>
        /// <param name="services">The IServiceCollection to add to.</param>
        /// <param name="timeZoneHeaderNames">
        /// A list of HTTP header names to inspect (in prioritized order) for a timezone value.
        /// </param>
        public static IServiceCollection AddTimeZoneAwareness(this IServiceCollection services, params string[] timeZoneHeaderNames)
        {
            // 1. Configure the options so the middleware can read HeaderNames
            services.Configure<TimeZoneOptions>(opts =>
            {
                opts.HeaderNames = timeZoneHeaderNames ?? [];
            });

            // 2. Register the middleware itself
            services.AddTransient<TimeZoneMiddleware>();

            return services;
        }

        /// <summary>
        /// Inserts TimeZoneMiddleware into the ASP.NET Core request pipeline.
        /// </summary>
        public static IApplicationBuilder UseTimeZoneAwareness(this IApplicationBuilder app)
        {
            return app.UseMiddleware<TimeZoneMiddleware>();
        }
    }
}
