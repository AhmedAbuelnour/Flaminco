using Microsoft.AspNetCore.Components;
using Microsoft.Playwright;

namespace Flaminco.RazorInk.Abstractions
{
    /// <summary>
    /// Provides functionality to render HTML content from Blazor components to a PDF file.
    /// </summary>
    public interface IPdfRazorInk
    {
        /// <summary>
        /// Renders a Blazor component to a PDF document.
        /// </summary>
        /// <typeparam name="TComponent">The type of the Blazor component to render.</typeparam>
        /// <param name="parameters">A dictionary of parameters to pass to the component.</param>
        /// <param name="pdfOptions">PDF options such as format, margins, etc.</param>
        /// <returns>A task that represents the asynchronous operation, containing the generated PDF as a byte array.</returns>
        Task<byte[]> RenderAsync<TComponent>(Dictionary<string, object?> parameters, PagePdfOptions pdfOptions) where TComponent : IComponent;
    }
}
