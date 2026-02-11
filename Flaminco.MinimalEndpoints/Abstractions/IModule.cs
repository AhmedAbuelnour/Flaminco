using Microsoft.AspNetCore.Routing;

namespace Flaminco.MinimalEndpoints.Abstractions;

/// <summary>
/// Represents a module that can add routes to an endpoint route builder.
/// </summary>
public interface IModule
{
    /// <summary>
    /// Adds routes from this module to the specified endpoint route builder.
    /// </summary>
    /// <param name="app">The endpoint route builder to add routes to</param>
    abstract static void AddRoutes(IEndpointRouteBuilder app);
}