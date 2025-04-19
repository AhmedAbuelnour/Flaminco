using Flaminco.MinimalEndpoints.Abstractions;
using Flaminco.MinimalEndpoints.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.MinimalEndpoints.Extensions;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app) where TEndpoint : IMinimalRouteEndpoint
    {
        AsyncServiceScope scope = app.ServiceProvider.CreateAsyncScope();

        // Resolve the endpoint from the DI container.
        scope.ServiceProvider.GetRequiredService<TEndpoint>().AddRoute(app);

        return app;
    }

    public static RouteHandlerBuilder AddLogging<TEndpoint>(this RouteHandlerBuilder builder) where TEndpoint : IMinimalRouteEndpoint
    {
        return builder.AddEndpointFilter<LoggingEndpointFilter<TEndpoint>>();
    }

    public static RouteHandlerBuilder AddValidator<TRequest>(this RouteHandlerBuilder builder) where TRequest : notnull
    {
        return builder.AddEndpointFilter<ValidationEndpointFilter<TRequest>>();
    }
}