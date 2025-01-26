using Flaminco.MinimalMediatR.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Flaminco.MinimalMediatR.Extensions;

public static class MinimalMediatRExtensions
{
    public static RouteHandlerBuilder MediateGet<TEndPointRequest>(this IEndpointRouteBuilder app, string template)
        where TEndPointRequest : IEndPointRequest
    {
        return app.MapGet(template, async (ISender _sender, [AsParameters] TEndPointRequest request, CancellationToken cancellationToken) => await _sender.Send(request, cancellationToken));
    }

    public static RouteHandlerBuilder MediatePost<TEndPointRequest>(this IEndpointRouteBuilder app, string template)
        where TEndPointRequest : IEndPointRequest
    {
        return app.MapPost(template, async (ISender _sender, [AsParameters] TEndPointRequest request, CancellationToken cancellationToken) => await _sender.Send(request, cancellationToken));
    }

    public static RouteHandlerBuilder MediatePut<TEndPointRequest>(this IEndpointRouteBuilder app, string template)
        where TEndPointRequest : IEndPointRequest
    {
        return app.MapPut(template, async (ISender _sender, [AsParameters] TEndPointRequest request, CancellationToken cancellationToken) => await _sender.Send(request, cancellationToken));
    }

    public static RouteHandlerBuilder MediateDelete<TEndPointRequest>(this IEndpointRouteBuilder app, string template)
        where TEndPointRequest : IEndPointRequest
    {
        return app.MapDelete(template, async (ISender _sender, [AsParameters] TEndPointRequest request, CancellationToken cancellationToken) => await _sender.Send(request, cancellationToken));
    }

    public static RouteHandlerBuilder MediatePatch<TEndPointRequest>(this IEndpointRouteBuilder app, string template)
        where TEndPointRequest : IEndPointRequest
    {
        return app.MapPatch(template, async (ISender _sender, [AsParameters] TEndPointRequest request, CancellationToken cancellationToken) => await _sender.Send(request, cancellationToken));
    }
}