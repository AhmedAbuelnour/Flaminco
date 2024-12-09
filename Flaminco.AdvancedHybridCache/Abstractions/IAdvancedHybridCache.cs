using Flaminco.AdvancedHybridCache.Models;
using Microsoft.Extensions.Caching.Hybrid;

namespace Flaminco.AdvancedHybridCache.Abstractions
{

    public interface IAdvancedHybridCache
    {
        ValueTask<TItem> GetOrCreateAsync<TItem>(CacheKey cacheKey, Func<CancellationToken, ValueTask<TItem>> createCallback, CancellationToken cancellationToken = default);


        ValueTask<TItem> GetOrCreateAsync<TItem>(CacheKey cacheKey, Func<CancellationToken, ValueTask<TItem>> createCallback, HybridCacheEntryOptions hybridCacheEntryOptions, CancellationToken cancellationToken = default);


        ValueTask RemoveAsync(CacheKey cacheKey, CancellationToken cancellationToken = default);

        ValueTask SetAsync<TItem>(CacheKey cacheKey, TItem value, CancellationToken cancellationToken = default);

        ValueTask SetAsync<TItem>(CacheKey cacheKey, TItem item, HybridCacheEntryOptions hybridCacheEntryOptions, CancellationToken cancellationToken = default);


        ValueTask<TItem?> TryGetAsync<TItem>(CacheKey cacheKey, CancellationToken cancellationToken = default);


        ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default);
    }
}
