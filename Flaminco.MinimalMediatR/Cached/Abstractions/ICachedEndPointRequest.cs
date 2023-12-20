using Flaminco.MinimalMediatR.Abstractions;

namespace Flaminco.MinimalMediatR.Cached.Abstractions
{
    public interface ICachedEndPointRequest : IEndPointRequest, ICachedQuery
    {

    }
}
