using Flaminco.AzureBus.AMQP.Models;
using Flaminco.RazorInk.Abstractions;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Consumers;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/pdf")]
    public class PdfController(IRazorInkPdfGenerator pdfGenerator, HelloPublisher amqpLocator) : ControllerBase
    {
        [HttpPost("generate")]
        public async Task<IActionResult> GeneratePdf()
        {
            await amqpLocator.PublishAsync(new MessageBox
            {
                Message = "Hellosss"
            }, new MessagePublishOptions
            {
                ApplicationProperties = { ["to"] = "something" }
            });

            return Ok();
        }
    }


}
