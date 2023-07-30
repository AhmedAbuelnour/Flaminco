using System.Threading;
using System.Threading.Tasks;

namespace Flaminco.Hangfire.Abstractions;

public interface IServiceJob
{
    ValueTask Execute(string? value = default, CancellationToken cancellationToken = default);
}
