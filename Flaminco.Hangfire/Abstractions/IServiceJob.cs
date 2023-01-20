using System.Threading.Tasks;

namespace Flaminco.Hangfire.Abstractions
{
    public interface IServiceJob
    {
        ValueTask Execute<TValue>(TValue? value = default) where TValue : IServiceValue;
    }
}
