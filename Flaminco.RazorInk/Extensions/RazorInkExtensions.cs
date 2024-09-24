using Flaminco.RazorInk.Abstractions;
using Flaminco.RazorInk.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.RazorInk.Extensions
{
    /// <summary>
    /// Contains extension methods to add RazorInk services to an ASP.NET Core application's dependency injection container.
    /// </summary>
    public static class RazorInkExtensions
    {
        /// <summary>
        /// Registers the PdfRazorInk as a singleton service in the dependency injection container.
        /// </summary>
        /// <param name="services">The IServiceCollection to add the service to.</param>
        /// <returns>The IServiceCollection with the RazorInk service registered.</returns>
        public static IServiceCollection AddRazorInk(this IServiceCollection services) => services.AddSingleton<IPdfRazorInk, PdfRazorInk>();
    }
}
