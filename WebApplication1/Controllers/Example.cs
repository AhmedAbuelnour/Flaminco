using Flaminco.ImmutableLookups.Abstractions;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Entities;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Example(IImmutableLookupQuery<WorkflowStatus, int> lookupQuery)
    {
        [HttpGet]
        public async Task GetLookups()
        {
            var lookups = await lookupQuery.GetByModuleAsync("Oss_submissions");

        }
    }


}
