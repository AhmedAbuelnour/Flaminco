using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Flaminco.MinimalEndpoints.Abstractions
{
    public interface IMinimalRouteEndpoint
    {
        void AddRoute(IEndpointRouteBuilder app);
    }

    public interface IMinimalEndpoint : IMinimalRouteEndpoint
    {
        ValueTask<IResult> Handle(CancellationToken cancellationToken);
    }

    public interface IMinimalEndpoint<TRequest> : IMinimalRouteEndpoint
    {
        ValueTask<IResult> Handle(TRequest request, CancellationToken cancellationToken);
    }

    public interface IMinimalEndpoint<TRequest, TResult> : IMinimalRouteEndpoint
    {
        ValueTask<TResult> Handle(TRequest request, CancellationToken cancellationToken);
    }

}
