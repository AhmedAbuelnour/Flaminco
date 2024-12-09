namespace Flaminco.AdvancedHybridCache.Models
{
    /// <summary>
    /// Represents a cache key with a region, key, and associated tags.
    /// </summary>
    public record struct CacheKey
    {
        /// <summary>
        /// Gets or sets the region of the cache.
        /// </summary>
        public required string Region { get; set; }

        /// <summary>
        /// Gets or sets the key within the region.
        /// </summary>
        public required string Key { get; set; }

        /// <summary>
        /// Gets or sets the collection of tags associated with the cache key.
        /// </summary>
        public IReadOnlyCollection<string>? Tags { get; set; }

        /// <summary>
        /// Returns a string representation of the cache key in the format "Region:Key".
        /// </summary>
        /// <returns>A string representation of the cache key.</returns>
        public override readonly string ToString() => $"{Region}:{Key}";

        /// <summary>
        /// Returns a string representation of the cache key including tags, if any, in the format "Region:Key:Tag1,Tag2,...".
        /// </summary>
        /// <returns>A string representation of the cache key including tags.</returns>
        public readonly string ToTagString() => Tags?.Count > 0 ? $"{ToString()}:{string.Join(',', Tags)}" : ToString();

        /// <summary>
        /// Implicitly converts a <see cref="CacheKey"/> to its string representation.
        /// </summary>
        /// <param name="cacheKey">The cache key to convert.</param>

        public static implicit operator string(CacheKey cacheKey) => cacheKey.ToString();
    }
}
