using Microsoft.AspNetCore.Routing;

namespace Flaminco.MinimalEndpoints.Abstractions
{
    /// <summary>
    /// Represents a minimal API endpoint that can be registered with the route builder.
    /// </summary>
    public interface IMinimalEndpoint
    {
        static abstract void AddRoute(IEndpointRouteBuilder app);
    }
}
