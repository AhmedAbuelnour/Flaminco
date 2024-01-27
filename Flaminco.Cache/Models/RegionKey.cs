namespace Flaminco.Cache.Models
{
    public record struct RegionKey
    {
        public required string Region { get; set; }
        public required string Key { get; set; }
        public override readonly string ToString() => $"{Region}:{Key}";
    }
}
