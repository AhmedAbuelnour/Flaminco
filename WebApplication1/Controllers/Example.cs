using Flaminco.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Consumers;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/pdf")]
public class ExampleController(HelloMessageFlow helloMessageFlow) : ControllerBase
{
    [HttpPost("greating")]
    public async Task<IActionResult> GenerateMessage()
    {
        Response<ExampleResponse> response = await helloMessageFlow.GetResponseAsync<ExampleResponse>(new ExampleRequest
        {
            Id = 1,
        });

        return Ok(response.Message);
    }
}