using System.Threading;
using System.Threading.Tasks;

namespace Flaminco.Hangfire.Abstractions
{
    public interface IServiceJob
    {
        ValueTask Execute<TValue>(TValue? value = default, CancellationToken cancellationToken = default) where TValue : IServiceValue;
    }
}
