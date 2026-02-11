using Flaminco.MinimalEndpoints.Implementations;
using Flaminco.MinimalEndpoints.Middlewares;
using Flaminco.MinimalEndpoints.RequestCultureProviders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Flaminco.MinimalEndpoints.Extensions;

public static class TextLocatorExtensions
{
    /// <summary>
    /// Adds JSON-based localization services using native IStringLocalizer.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The localization options containing ResourcesPath.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddJsonLocalization(this IServiceCollection services, Action<LocalizationOptions> options)
    {
        services.Configure(options);

        // Register the factory and localizer using native interfaces
        services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();

        services.AddSingleton(sp => sp.GetRequiredService<IStringLocalizerFactory>().Create(typeof(object)));

        services.AddTransient<LocalizationContextMiddleware>();

        return services;
    }

    /// <summary>
    /// Configures the application to use JSON-based localization with the specified supported cultures.
    /// The first passed culture will be used as the default culture.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="supportedCultures">The supported cultures (base language codes like "en", "ar").</param>
    /// <returns>The updated application builder.</returns>
    public static IApplicationBuilder UseJsonLocalization(this IApplicationBuilder app, params string[] supportedCultures)
    {
        string defaultLanguage = supportedCultures.FirstOrDefault() ?? "en";

        var requestLocalizationOptions = new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(defaultLanguage),
            SupportedCultures = [.. supportedCultures.Select(c => new CultureInfo(c))],
            SupportedUICultures = [.. supportedCultures.Select(c => new CultureInfo(c))]
        };

        // Add custom culture provider that normalizes language codes
        requestLocalizationOptions.RequestCultureProviders.Insert(0, new LanguageRequestCultureProvider(defaultLanguage));

        app.UseRequestLocalization(requestLocalizationOptions);
        app.UseMiddleware<LocalizationContextMiddleware>();

        return app;
    }
}
