using System.Threading.Tasks;

namespace Flaminco.Hangfire.Abstractions
{
    public interface IServiceJob
    {
        ValueTask Execute();
    }
}
