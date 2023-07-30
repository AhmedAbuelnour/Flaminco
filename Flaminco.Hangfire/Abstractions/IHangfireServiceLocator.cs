using System.Threading;

namespace Flaminco.Hangfire.Abstractions;

public interface IHangfireServiceLocator
{
    string? Execute<TServiceJob>(string? value = default, string? parentId = default, CancellationToken cancellationToken = default) where TServiceJob : IServiceJob;
}
