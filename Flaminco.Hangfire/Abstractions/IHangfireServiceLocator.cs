namespace Flaminco.Hangfire.Abstractions
{
    public interface IHangfireServiceLocator
    {
        string? Execute<TServiceJob>(string? parentId = default) where TServiceJob : IServiceJob;
    }
}
