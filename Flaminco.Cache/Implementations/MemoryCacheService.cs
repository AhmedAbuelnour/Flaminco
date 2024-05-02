using Flaminco.Cache.Abstracts;
using Flaminco.Cache.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Flaminco.Cache.Implementations
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly MemoryCacheEntryOptions _cacheOptions;
        private readonly CacheConfiguration? _cacheConfig;
        public MemoryCacheService(IMemoryCache memoryCache,
                                  IOptions<CacheConfiguration> cacheConfig)
        {
            _memoryCache = memoryCache;
            _cacheConfig = cacheConfig.Value;
            _cacheOptions = _cacheConfig is null ? new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60)
            } : new MemoryCacheEntryOptions
            {
                SlidingExpiration = _cacheConfig.SlidingExpiration.HasValue ? TimeSpan.FromMinutes(_cacheConfig.SlidingExpiration.Value) : null,
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheConfig.AbsoluteExpiration)
            };
        }


        public Task<TItem?> TryGetAsync<TItem>(RegionKey regionKey,
                                                     CancellationToken cancellationToken = default)
        {
            return _memoryCache.TryGetValue(regionKey.ToString(), out TItem? cachedItem) ? Task.FromResult(cachedItem) : Task.FromResult<TItem?>(default);
        }

        public Task RemoveAsync(RegionKey regionKey,
                                CancellationToken cancellationToken = default)
        {
            _memoryCache.Remove(regionKey.ToString());

            return Task.CompletedTask;
        }

        public Task SetAsync<TItem>(RegionKey regionKey,
                                    TItem value,
                                    CancellationToken cancellationToken = default)
        {
            _memoryCache.Set(regionKey.ToString(), value, _cacheOptions);

            return Task.CompletedTask;
        }

        public Task SetAsync<TItem>(RegionKey regionKey,
                                    TItem value,
                                    TimeSpan? absoluteExpirationRelativeToNow,
                                    TimeSpan? slidingExpiration,
                                    CancellationToken cancellationToken = default)
        {
            _memoryCache.Set(regionKey.ToString(), value, new MemoryCacheEntryOptions
            {
                SlidingExpiration = slidingExpiration,
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow
            });

            return Task.CompletedTask;
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

            return Task.CompletedTask;
        }
    }
}
