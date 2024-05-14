using Flaminco.Cache.Abstracts;
using Microsoft.Extensions.Caching.Hybrid;

namespace Flaminco.Cache.Implementations
{
    public class DefaultHybridCache : IHybridCache
    {
        private readonly HybridCache _hybridCache;

        public DefaultHybridCache(HybridCache hybridCache)
        {
            _hybridCache = hybridCache;
        }


    }
}
