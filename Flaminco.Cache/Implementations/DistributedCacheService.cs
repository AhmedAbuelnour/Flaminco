using Flaminco.Cache.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Flaminco.Cache.Implementations
{
    public class DistributedCacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly DistributedCacheEntryOptions _cacheOptions;
        private readonly CacheConfiguration? _cacheConfig;
        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
        public DistributedCacheService(IDistributedCache distributedCache, IOptions<CacheConfiguration> cacheConfig)
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
            if (await _distributedCache.GetAsync(regionKey.ToString(), cancellationToken) is byte[] cachedValue)
            {
                return JsonSerializer.Deserialize<TItem>(new ReadOnlySpan<byte>(cachedValue))!;
            }

            return default;
        }

        public async Task<TItem> GetOrCreateAsync<TItem>(RegionKey regionKey, Func<CancellationToken, Task<TItem>> createCallback, CancellationToken cancellationToken = default)
        {
            if (await _distributedCache.GetAsync(regionKey.ToString(), cancellationToken) is byte[] cachedValue)
            {
                return JsonSerializer.Deserialize<TItem>(new ReadOnlySpan<byte>(cachedValue))!;
            }
            else
            {
                TItem? callbackResult = await createCallback(cancellationToken);

                ArgumentNullException.ThrowIfNull(nameof(callbackResult));

                await SetAsync(regionKey, callbackResult, cancellationToken);

                return callbackResult;
            }

        }

        public async Task<TItem> GetOrCreateAsync<TItem>(RegionKey regionKey, Func<CancellationToken, Task<TItem>> createCallback, TimeSpan? absoluteExpirationRelativeToNow, TimeSpan? slidingExpiration, CancellationToken cancellationToken = default)
        {
            if (await _distributedCache.GetAsync(regionKey.ToString(), cancellationToken) is byte[] cachedValue)
            {
                return JsonSerializer.Deserialize<TItem>(new ReadOnlySpan<byte>(cachedValue))!;
            }
            else
            {
                TItem? callbackResult = await createCallback(cancellationToken);

                ArgumentNullException.ThrowIfNull(nameof(callbackResult));

                await SetAsync(regionKey, callbackResult, absoluteExpirationRelativeToNow, slidingExpiration, cancellationToken);

                return callbackResult;
            }
        }

        public Task SetAsync<TItem>(RegionKey regionKey, TItem item, CancellationToken cancellationToken = default)
        {
            byte[] valueJson = JsonSerializer.SerializeToUtf8Bytes(item, _jsonSerializerOptions);

            return _distributedCache.SetAsync(regionKey.ToString(), valueJson, _cacheOptions, cancellationToken);
        }

        public Task SetAsync<TItem>(RegionKey regionKey, TItem item, TimeSpan? absoluteExpirationRelativeToNow, TimeSpan? slidingExpiration, CancellationToken cancellationToken = default)
        {
            byte[] valueJson = JsonSerializer.SerializeToUtf8Bytes(item, _jsonSerializerOptions);

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


        private DistributedCacheEntryOptions GetCacheOptions()
        {
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions();

            if (_cacheConfig != null)
            {
                if (_cacheConfig.SlidingExpiration.HasValue)
                {
                    options.SetSlidingExpiration(TimeSpan.FromMinutes(_cacheConfig.SlidingExpiration.Value));
                }

                options.SetAbsoluteExpiration(TimeSpan.FromMinutes(_cacheConfig.AbsoluteExpiration));
            }
            else
            {
                // Default configuration if _cacheConfig is null
                options.SetSlidingExpiration(TimeSpan.FromMinutes(30)); // Default sliding expiration

                options.SetAbsoluteExpiration(TimeSpan.FromMinutes(60)); // Default absolute expiration
            }

            return options;
        }

    }
}
