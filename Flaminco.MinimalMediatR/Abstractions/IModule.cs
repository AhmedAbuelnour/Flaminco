using Microsoft.AspNetCore.Routing;

namespace Flaminco.MinimalMediatR.Abstractions;

public interface IModule
{
    void AddRoutes(IEndpointRouteBuilder app);
}