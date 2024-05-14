namespace Flaminco.CacheKeys
{
    /// <summary>
    /// Represents a unique key for caching mechanisms that includes a region, key identifier, and an optional set of tags.
    /// This structure supports implicit conversion to a string and provides a custom string representation.
    /// </summary>
    public record struct CacheKey
    {
        /// <summary>
        /// Gets or sets the region associated with the cache key.
        /// </summary>
        public required string Region { get; set; }

        /// <summary>
        /// Gets or sets the actual key identifier within the specified region.
        /// </summary>
        public required string Key { get; set; }

        /// <summary>
        /// Gets or sets a collection of tags that may be associated with the cache key.
        /// </summary>
        public IReadOnlyCollection<string>? Tags { get; set; }

        /// <summary>
        /// Provides a custom string representation of the cache key formatted as "Region:Key".
        /// </summary>
        /// <returns>A string that represents the current cache key.</returns>
        public override readonly string ToString() => $"{Region}:{Key}";

        /// <summary>
        /// Allows implicit conversion of the CacheKey structure to a string.
        /// </summary>
        /// <param name="cacheKey">The CacheKey to convert.</param>
        /// <returns>A string that represents the current cache key.</returns>
        public static implicit operator string(CacheKey cacheKey) => cacheKey.ToString();
    }
}
