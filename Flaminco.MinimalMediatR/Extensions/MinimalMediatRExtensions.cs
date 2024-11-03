using Flaminco.MinimalMediatR.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Flaminco.MinimalMediatR.Extensions;

public static class MinimalMediatRExtensions
{
    public static RouteHandlerBuilder MediateGet<TEndPointRequest>(this IEndpointRouteBuilder app, string template)
        where TEndPointRequest : IEndPoint
    {
        return app.MapGet(template,
            (ISender _sender, [AsParameters] TEndPointRequest request, CancellationToken cancellationToken)
                => _sender.Send(request, cancellationToken));
    }

    public static RouteHandlerBuilder MediatePost<TEndPointRequest>(this IEndpointRouteBuilder app, string template)
        where TEndPointRequest : IEndPoint
    {
        return app.MapPost(template,
            (ISender _sender, [AsParameters] TEndPointRequest request, CancellationToken cancellationToken)
                => _sender.Send(request, cancellationToken));
    }

    public static RouteHandlerBuilder MediatePut<TEndPointRequest>(this IEndpointRouteBuilder app, string template)
        where TEndPointRequest : IEndPoint
    {
        return app.MapPut(template,
            (ISender _sender, [AsParameters] TEndPointRequest request, CancellationToken cancellationToken)
                => _sender.Send(request, cancellationToken));
    }

    public static RouteHandlerBuilder MediateDelete<TEndPointRequest>(this IEndpointRouteBuilder app, string template)
        where TEndPointRequest : IEndPoint
    {
        return app.MapDelete(template,
            (ISender _sender, [AsParameters] TEndPointRequest request, CancellationToken cancellationToken)
                => _sender.Send(request, cancellationToken));
    }

    public static RouteHandlerBuilder MediatePatch<TEndPointRequest>(this IEndpointRouteBuilder app, string template)
        where TEndPointRequest : IEndPoint
    {
        return app.MapPatch(template,
            (ISender _sender, [AsParameters] TEndPointRequest request, CancellationToken cancellationToken)
                => _sender.Send(request, cancellationToken));
    }
}