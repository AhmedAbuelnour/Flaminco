namespace Flaminco.MinimalMediatR.Cached.Abstractions
{
    public interface ICachedQuery
    {
        string Key { get; }
        TimeSpan? Expiration { get; }
    }
}
