namespace Flaminco.Cache.Models
{
    public class RegionKey
    {
        public required string Region { get; set; }
        public required string Key { get; set; }

        public override string ToString() => $"{Region}:{Key}";
    }
}
