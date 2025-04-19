using Microsoft.AspNetCore.Routing;

namespace Flaminco.MinimalEndpoints.Abstractions;

public interface IModule
{
    void AddRoutes(IEndpointRouteBuilder app);
}