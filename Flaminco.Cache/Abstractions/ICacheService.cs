using Flaminco.Cache.Models;

namespace Flaminco.Cache
{
    public interface ICacheService
    {
        Task<TItem> GetOrCreateAsync<TItem>(RegionKey regionKey, Func<Task<TItem>> createCallback, CancellationToken cancellationToken = default);
        Task RemoveAsync(RegionKey regionKey, CancellationToken cancellationToken = default);
        Task SetAsync<TItem>(RegionKey regionKey, TItem value, CancellationToken cancellationToken = default);
        Task SetAsync<TItem>(RegionKey regionKey, TItem value, TimeSpan? absoluteExpirationRelativeToNow, TimeSpan? slidingExpiration, CancellationToken cancellationToken = default);
        Task<TItem?> TryGetAsync<TItem>(RegionKey regionKey, CancellationToken cancellationToken = default);
    }
}
