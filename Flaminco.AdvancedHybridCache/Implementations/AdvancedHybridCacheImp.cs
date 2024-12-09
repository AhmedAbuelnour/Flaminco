using Flaminco.AdvancedHybridCache.Abstractions;
using Flaminco.AdvancedHybridCache.Models;
using Microsoft.Extensions.Caching.Hybrid;

namespace Flaminco.AdvancedHybridCache.Implementations
{
    public sealed class AdvancedHybridCacheImp(HybridCache _hybridCache) : IAdvancedHybridCache
    {
        public async ValueTask<TItem?> TryGetAsync<TItem>(CacheKey cacheKey,
                                                    CancellationToken cancellationToken = default)
        {
            return await _hybridCache.GetOrCreateAsync(cacheKey, (token) => ValueTask.FromResult<TItem?>(default), new HybridCacheEntryOptions
            {
                Flags = HybridCacheEntryFlags.DisableUnderlyingData | HybridCacheEntryFlags.DisableLocalCacheWrite | HybridCacheEntryFlags.DisableDistributedCacheWrite
            }, cacheKey.Tags, cancellationToken: cancellationToken);
        }

        public async ValueTask<TItem> GetOrCreateAsync<TItem>(CacheKey cacheKey,
                                                        Func<CancellationToken, ValueTask<TItem>> createCallback,
                                                        CancellationToken cancellationToken = default)
        {
            return await _hybridCache.GetOrCreateAsync(cacheKey, createCallback, tags: cacheKey.Tags, cancellationToken: cancellationToken);
        }


        public async ValueTask<TItem> GetOrCreateAsync<TItem>(CacheKey cacheKey,
                                                        Func<CancellationToken, ValueTask<TItem>> createCallback,
                                                        HybridCacheEntryOptions hybridCacheEntryOptions,
                                                        CancellationToken cancellationToken = default)
        {
            return await _hybridCache.GetOrCreateAsync(cacheKey, createCallback, hybridCacheEntryOptions, cacheKey.Tags, cancellationToken);
        }

        public async ValueTask SetAsync<TItem>(CacheKey cacheKey,
                                         TItem item,
                                         CancellationToken cancellationToken = default)
        {

            await _hybridCache.SetAsync(cacheKey, item, tags: cacheKey.Tags, cancellationToken: cancellationToken);
        }

        public async ValueTask SetAsync<TItem>(CacheKey cacheKey,
                                         TItem item,
                                         HybridCacheEntryOptions hybridCacheEntryOptions,
                                         CancellationToken cancellationToken = default)
        {

            await _hybridCache.SetAsync(cacheKey, item, hybridCacheEntryOptions, cacheKey.Tags, cancellationToken);
        }

        public async ValueTask RemoveAsync(CacheKey cacheKey,
                                     CancellationToken cancellationToken = default)
        {
            await _hybridCache.RemoveAsync(cacheKey, cancellationToken);
        }

        public async ValueTask RemoveByTagAsync(string tag,
                                          CancellationToken cancellationToken = default)
        {
            await _hybridCache.RemoveByTagAsync(tag, cancellationToken);
        }
    }
}
