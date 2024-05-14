using Flaminco.Cache.Models;
using Microsoft.Extensions.Caching.Hybrid;

namespace Flaminco.Cache.Abstracts
{
    public interface IHybridCache
    {
        Task<TItem> GetOrCreateAsync<TItem>(RegionKey regionKey, Func<CancellationToken, Task<TItem>> createCallback, CancellationToken cancellationToken = default);
        Task<TItem> GetOrCreateAsync<TItem>(RegionKey regionKey, Func<CancellationToken, Task<TItem>> createCallback, HybridCacheEntryOptions hybridCacheEntryOptions, CancellationToken cancellationToken = default);
        Task RemoveAllAsync(CancellationToken cancellationToken = default);
        Task RemoveAsync(RegionKey regionKey, CancellationToken cancellationToken = default);
        Task SetAsync<TItem>(RegionKey regionKey, TItem value, CancellationToken cancellationToken = default);
        Task SetAsync<TItem>(RegionKey regionKey, TItem value, TimeSpan? absoluteExpirationRelativeToNow, TimeSpan? slidingExpiration, CancellationToken cancellationToken = default);
        Task<TItem?> TryGetAsync<TItem>(RegionKey regionKey, CancellationToken cancellationToken = default);
    }
}
