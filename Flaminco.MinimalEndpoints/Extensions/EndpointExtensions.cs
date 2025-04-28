using Flaminco.MinimalEndpoints.Abstractions;
using Flaminco.MinimalEndpoints.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.MinimalEndpoints.Extensions;

/// <summary>
/// Provides extension methods for configuring and registering minimal endpoints.
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Maps a minimal endpoint to the endpoint route builder.
    /// </summary>
    /// <typeparam name="TEndpoint">The type of endpoint to map</typeparam>
    /// <param name="app">The endpoint route builder</param>
    /// <returns>The endpoint route builder for method chaining</returns>
    public static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app) where TEndpoint : IMinimalRouteEndpoint
    {
        AsyncServiceScope scope = app.ServiceProvider.CreateAsyncScope();

        // Resolve the endpoint from the DI container.
        scope.ServiceProvider.GetRequiredService<TEndpoint>().AddRoute(app);

        return app;
    }

    /// <summary>
    /// Adds logging functionality to an endpoint.
    /// </summary>
    /// <typeparam name="TEndpoint">The type of endpoint to add logging to</typeparam>
    /// <param name="builder">The route handler builder</param>
    /// <returns>The route handler builder with logging filter added</returns>
    public static RouteHandlerBuilder AddLogging<TEndpoint>(this RouteHandlerBuilder builder) where TEndpoint : IMinimalRouteEndpoint
    {
        return builder.AddEndpointFilter<LoggingEndpointFilter<TEndpoint>>();
    }

    /// <summary>
    /// Adds request validation functionality to an endpoint.
    /// </summary>
    /// <typeparam name="TRequest">The type of request to validate</typeparam>
    /// <param name="builder">The route handler builder</param>
    /// <returns>The route handler builder with validation filter added</returns>
    public static RouteHandlerBuilder AddValidator<TRequest>(this RouteHandlerBuilder builder) where TRequest : notnull
    {
        return builder.AddEndpointFilter<ValidationEndpointFilter<TRequest>>();
    }
}