using Flaminco.CacheKeys;
using Flaminco.ImmutableLookups.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;
using WebApplication1.Entities;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Example(IImmutableLookupQuery<WorkflowStatus, int> lookupQuery)
    {
        private static CacheKey LookupsOsssubmission = new CacheKey
        {
            Region = "Lookups",
            Key = "oss_submissions",
            Tags = ["lookups", "submissions"]
        };

        [HttpGet]
        public async Task GetLookups(HybridCache hybridCache, CancellationToken cancellationToken)
        {
            List<WorkflowStatus> workflowStatuses = await hybridCache.GetOrCreateAsync(LookupsOsssubmission, async (token) => await lookupQuery.GetByModuleAsync("Oss_submissions", token), tags: LookupsOsssubmission.Tags, token: cancellationToken);

        }
    }


}
