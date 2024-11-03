# Flaminco.RazorInk

`Flaminco.RazorInk` is designed to simplify the process of generating PDF based on Razor

## Installation

```shell
dotnet add package Flaminco.RazorInk
```

## Getting Started

### Installation

#### Step 1: Add Migration to Your Services

To get started with `Flaminco.RazorInk`, you need to configure the Migration services in your ASP.NET Core application.
You can do this by adding the following line:

Using Direct Configuration

```csharp

builder.Services.AddRazorInk();
```

#### Step 2: Inject `IRazorInkPdfGenerator`

```csharp
[ApiController]
[Route("api/pdf")]
public class PdfController : ControllerBase
{
    private readonly IPdfRazorInk _pdfGenerator;

    public PdfController(IPdfRazorInk pdfGenerator)
    {
        _pdfGenerator = pdfGenerator;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GeneratePdf([FromBody] Invoice invoiceModel)
    {
        PagePdfOptions pdfOptions = new PagePdfOptions
        {
            Format = "A4",
            DisplayHeaderFooter = true,
            HeaderTemplate = "<div style='font-size:12px; text-align:center; width:100%;'>Invoice Header</div>",
            FooterTemplate = "<div style='font-size:12px; text-align:center; width:100%;'>Page <span class='pageNumber'></span> of <span class='totalPages'></span></div>",
            PrintBackground = true,
        };

        byte[] pdfBytes = await _pdfGenerator.RenderAsync<InvoiceComponent>(new Dictionary<string, object?>
        {
            { "Invoice", invoiceModel }
        }, pdfOptions);

        return File(pdfBytes, "application/pdf", "document.pdf");
    }
}

```

```html

@code {
    [Parameter] public Invoice Invoice { get; set; } // Note that the property name must matchs the key name in the Dictionary
}

<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8"/>
    <title>Invoice @Invoice.Number</title>

    <style>
        /* Your Style */
	</style>
</head>

<body>
    <div class="invoice-box page-break">
    <table cellpadding="0" cellspacing="0">
        <tr class="top">
            <td colspan="2">
                <table>
                    <tr>
                        <td>
                            Invoice #: @Invoice.Number<br/>
                            Created: @Invoice.IssueDate.ToString("MMMM dd, yyyy")<br/>
                            Due: @Invoice.DueDate.ToString("MMMM dd, yyyy")
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
		<!-- The Rest of your HTML Code. -->
    </table>
</div>
</body>
</html>

```

### Placeholders:

* `<span class='pageNumber'></span>` this placeholder is for writing the current page number
* `<span class='totalPages'></span>` this placeholder is for writing the total page numbers.

### Containerize

Replace `FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base` with
`FROM mcr.microsoft.com/playwright/dotnet:v1.46.0-jammy AS base`

Where v1.46.0 matches the current playwright

Where you can also check the last docker image for dotnet
from [here](https://hub.docker.com/r/microsoft/playwright-dotnet)

### Contributing

Contributions are welcome! If you find any issues or have suggestions for improvements, please open an issue or submit a
pull request.

### License

This project is licensed under the MIT License.
