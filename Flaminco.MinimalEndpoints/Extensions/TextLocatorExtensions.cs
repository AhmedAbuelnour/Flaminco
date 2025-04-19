using Flaminco.MinimalEndpoints.Implementations;
using Flaminco.MinimalEndpoints.RequestCultureProviders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Flaminco.MinimalEndpoints.Extensions
{
    public static class TextLocatorExtensions
    {
        /// <summary>
        /// Adds the text locator service to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="options">The localization options.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddTextLocator(this IServiceCollection services, Action<LocalizationOptions> options)
        {
            services.Configure(options);

            services.AddSingleton<IStringLocalizer, DefaultTextLocator>();

            return services;
        }


        /// <summary>
        /// Configures the application to use the text locator with the specified supported cultures.
        /// The first passed culture will be used as the default culture.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="supportedCultures">The supported cultures.</param>
        /// <returns>The updated application builder.</returns>
        public static IApplicationBuilder UseTextLocator(this IApplicationBuilder app, params string[] supportedCultures)
        {
            string defaultLanguage = supportedCultures.FirstOrDefault() ?? "en";

            var requestLocalizationOptions = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(defaultLanguage),
                SupportedCultures = [.. supportedCultures.Select(c => new CultureInfo(c))],
                SupportedUICultures = [.. supportedCultures.Select(c => new CultureInfo(c))]
            };

            // Add custom culture provider
            requestLocalizationOptions.RequestCultureProviders.Insert(0, new LanguageRequestCultureProvider(defaultLanguage));

            app.UseRequestLocalization(requestLocalizationOptions);

            return app;
        }
    }
}
