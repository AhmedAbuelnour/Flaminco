using Flaminco.Cache.Models;

namespace Flaminco.Cache.Implementations
{
    public sealed class CacheKeyService
    {
        private readonly HashSet<RegionKey> _cacheKeys = new();
        public void Add(RegionKey regionKey) => _cacheKeys.Add(regionKey);
        public void Remove(RegionKey regionKey) => _cacheKeys.Remove(regionKey);
        public void ClearKeys() => _cacheKeys.Clear();
        public IEnumerable<RegionKey> GetKeys() => _cacheKeys;
    }
}
