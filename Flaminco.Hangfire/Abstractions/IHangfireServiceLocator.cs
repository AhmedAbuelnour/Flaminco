namespace Flaminco.Hangfire.Abstractions
{
    public interface IHangfireServiceLocator
    {
        string? Execute<TServiceJob, TServiceValue>(TServiceValue? value = default, string? parentId = default) where TServiceJob : IServiceJob
                                                                                                                where TServiceValue : IServiceValue;

        string? Execute<TServiceJob>(string? parentId = default) where TServiceJob : IServiceJob;
    }
}
