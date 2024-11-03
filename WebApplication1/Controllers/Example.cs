using Flaminco.RazorInk.Abstractions;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Consumers;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/pdf")]
public class PdfController(IRazorInkPdfGenerator pdfGenerator, HelloPublisher amqpLocator) : ControllerBase
{
    [HttpPost("generate")]
    public async Task<IActionResult> GeneratePdf()
    {
        await amqpLocator.PublishAsync(new MessageBox
        {
            NotifierId = "b709e1eb-5050-44b4-914b-c772458308c0",
            NotifiedIds = ["11a6467b-6ecf-45df-a3c3-71023809f65f", "1842141f-41a7-4898-9b03-8dfe57ed9440"],
            CourseId = 7480,
            NotificationTypeId = 9,
            Metadata = "1501"
        });

        return Ok();
    }
}