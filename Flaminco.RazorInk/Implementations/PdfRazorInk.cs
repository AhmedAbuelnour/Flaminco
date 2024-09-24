using Flaminco.RazorInk.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;


namespace Flaminco.RazorInk.Implementations
{
    /// <summary>
    /// A service that generates PDF files from rendered Blazor components.
    /// </summary>
    /// <param name="_serviceProvider">The service provider used to resolve component dependencies.</param>
    /// <param name="_loggerFactory">The logger factory for logging operations within the PDF generation process</param>
    public class PdfRazorInk(IServiceProvider _serviceProvider, ILoggerFactory _loggerFactory) : IPdfRazorInk
    {
        /// <summary>
        /// Ensures that Playwright and the required browser binaries (Chromium) are installed.
        /// </summary>
        public static void EnsureInstalled()
        {
            if (!Directory.Exists("./playwright"))
            {
                Program.Main(["install", "chromium"]);
            }
        }

        /// <inheritdoc/>
        public async Task<byte[]> RenderAsync<TComponent>(Dictionary<string, object?> parameters, PagePdfOptions pdfOptions) where TComponent : IComponent
        {
            await using HtmlRenderer htmlRenderer = new(_serviceProvider, _loggerFactory);
            string html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
            {
                var output = await htmlRenderer.RenderComponentAsync<TComponent>(ParameterView.FromDictionary(parameters));

                return output.ToHtmlString();
            });

            using IPlaywright playwright = await Playwright.CreateAsync();
            await using IBrowser browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            IPage page = await browser.NewPageAsync();

            await page.SetContentAsync(html);

            byte[] pdfBytes = await page.PdfAsync(pdfOptions);

            await page.CloseAsync();

            await browser.CloseAsync();

            return pdfBytes;
        }
    }
}