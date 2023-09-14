using Flaminco.Cache.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Flaminco.Cache.Implementations
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly CacheConfiguration? _cacheConfig;
        private readonly DistributedCacheEntryOptions _cacheOptions;

        public CacheService(IDistributedCache distributedCache, IOptions<CacheConfiguration> cacheConfig)
        {
            _distributedCache = distributedCache;
            _cacheConfig = cacheConfig.Value;
            _cacheOptions = _cacheConfig is null ? new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60)
            } : new DistributedCacheEntryOptions
            {
                SlidingExpiration = _cacheConfig.SlidingExpiration.HasValue ? TimeSpan.FromMinutes(_cacheConfig.SlidingExpiration.Value) : null,
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheConfig.AbsoluteExpiration)
            };
        }

        public async Task<TItem?> TryGetAsync<TItem>(RegionKey regionKey, CancellationToken cancellationToken = default)
        {
            byte[]? cachedValue = await _distributedCache.GetAsync(regionKey.ToString(), cancellationToken);

            if (cachedValue is null)
            {
                return default;
            }

            return JsonSerializer.Deserialize<TItem>(new ReadOnlySpan<byte>(cachedValue));
        }

        public async Task<TItem> GetOrCreateAsync<TItem>(RegionKey regionKey, Func<Task<TItem>> createCallback, CancellationToken cancellationToken = default)
        {
            byte[]? cachedValue = await _distributedCache.GetAsync(regionKey.ToString(), cancellationToken);

            if (cachedValue is null)
            {
                TItem? callbackResult = await createCallback();

                if (callbackResult is null)
                {
                    ArgumentNullException.ThrowIfNull(nameof(callbackResult));
                }

                await SetAsync(regionKey, callbackResult, cancellationToken);

                return callbackResult;
            }

            return JsonSerializer.Deserialize<TItem>(new ReadOnlySpan<byte>(cachedValue))!;
        }


        public Task SetAsync<TItem>(RegionKey regionKey, TItem item, CancellationToken cancellationToken = default)
        {
            byte[] valueJson = JsonSerializer.SerializeToUtf8Bytes(item, new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            });

            return _distributedCache.SetAsync(regionKey.ToString(), valueJson, _cacheOptions, cancellationToken);
        }

        public Task SetAsync<TItem>(RegionKey regionKey, TItem item, TimeSpan? absoluteExpirationRelativeToNow, TimeSpan? slidingExpiration, CancellationToken cancellationToken = default)
        {
            byte[] valueJson = JsonSerializer.SerializeToUtf8Bytes(item, new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            });

            return _distributedCache.SetAsync(regionKey.ToString(), valueJson, new DistributedCacheEntryOptions
            {
                SlidingExpiration = slidingExpiration,
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow
            }, cancellationToken);
        }

        public Task RemoveAsync(RegionKey regionKey, CancellationToken cancellationToken = default)
        {
            return _distributedCache.RemoveAsync(regionKey.ToString(), cancellationToken);
        }
    }
}
