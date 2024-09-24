using Flaminco.RabbitMQ.AMQP.Abstractions;
using Flaminco.RazorInk.Abstractions;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Publishers;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/pdf")]
    public class PdfController : ControllerBase
    {
        private readonly IRazorInkPdfGenerator _pdfGenerator;
        private readonly IAMQPLocator _amqpLocator;
        public PdfController(IRazorInkPdfGenerator pdfGenerator, IAMQPLocator amqpLocator)
        {
            _pdfGenerator = pdfGenerator;
            _amqpLocator = amqpLocator;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GeneratePdf()
        {

            if (_amqpLocator.GetPublisher<PersonPublisher>() is PersonPublisher publisher)
            {
                await publisher.PublishAsync("Hellos");
            }

            return Ok();
        }
    }

}
