namespace Flaminco.Cache.Models
{
    public record RegionKey(string Region, string Key)
    {
        public override string ToString() => $"{Region}:{Key}";
    }
}
