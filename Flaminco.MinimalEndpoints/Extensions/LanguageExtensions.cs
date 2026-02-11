using Flaminco.MinimalEndpoints.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.MinimalEndpoints.Extensions
{
    public static class LanguageExtensions
    {
        public static IServiceCollection AddLanguageAwareness(this IServiceCollection services, string defaultLanguage = "en")
        {
            services.AddTransient((_) => new LanguageContextMiddleware(defaultLanguage));

            return services;
        }

        public static IApplicationBuilder UseLanguageAwareness(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LanguageContextMiddleware>();
        }
    }
}
