using Flaminco.Cache.Abstracts;
using Flaminco.Cache.Models;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;

namespace Flaminco.Cache.Implementations
{
    public class MemoryCacheService : ICacheService
    {
        private readonly HybridCache _hybridCache;

        public ValueTask RemoveAsync(RegionKey regionKey, CancellationToken cancellationToken = default)
        {
            return _hybridCache.RemoveKeyAsync(regionKey.ToString(), cancellationToken);
        }

        public ValueTask SetAsync<TItem>(RegionKey regionKey, TItem value, CancellationToken cancellationToken = default)
        {
            return _hybridCache.SetAsync(regionKey.ToString(), value, token: cancellationToken);
        }

        public ValueTask SetAsync<TItem>(RegionKey regionKey,
                                     TItem value,
                                    HybridCacheEntryOptions options,
                                    CancellationToken cancellationToken = default)
        {
            return _hybridCache.SetAsync(regionKey.ToString(), value, options, token: cancellationToken);
        }

        public Task<TItem> GetOrCreateAsync<TItem>(RegionKey regionKey,
                                                   Func<CancellationToken, Task<TItem>> createCallback,
                                                   CancellationToken cancellationToken = default)
        {
            return _memoryCache.GetOrCreateAsync(regionKey.ToString(), (entity) =>
            {
                entity.SetOptions(_cacheOptions);

                return createCallback(cancellationToken);
            })!;
        }

        public Task<TItem> GetOrCreateAsync<TItem>(RegionKey regionKey,
                                                   Func<CancellationToken, Task<TItem>> createCallback,
                                                   TimeSpan? absoluteExpirationRelativeToNow,
                                                   TimeSpan? slidingExpiration,
                                                   CancellationToken cancellationToken = default)
        {
            return _memoryCache.GetOrCreateAsync(regionKey.ToString(), (entity) =>
            {
                entity.SetOptions(new MemoryCacheEntryOptions
                {
                    SlidingExpiration = slidingExpiration,
                    AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow
                });

                return createCallback(cancellationToken);
            })!;
        }

        public Task RemoveAllAsync(CancellationToken cancellationToken = default)
        {
            if (_memoryCache is MemoryCache concreteMemoryCache)
            {
                concreteMemoryCache.Clear();
            }

            _hybridCache.removeA

            return Task.CompletedTask;
        }
    }
}
