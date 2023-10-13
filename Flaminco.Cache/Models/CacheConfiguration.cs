namespace Flaminco.Cache.Models
{
    public class CacheConfiguration
    {
        public string Mechanism { get; set; } = default!;
        public int AbsoluteExpiration { get; set; }
        public int? SlidingExpiration { get; set; }
        public string? RedisConnectionString { get; set; }
        public string? InstanceName { get; set; }
    }
}
