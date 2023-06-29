﻿using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Flaminco.MinimalMediatR;

public static class MinimalMediatRExtensions
{
    public static RouteHandlerBuilder MediateGet<IHttpRequest>(this IEndpointRouteBuilder app, string template) where IHttpRequest : notnull
        => app.MapGet(template, async (ISender _sender, [AsParameters] IHttpRequest request, CancellationToken cancellationToken) => await _sender.Send(request, cancellationToken));

    public static RouteHandlerBuilder MediatePost<IHttpRequest>(this IEndpointRouteBuilder app, string template) where IHttpRequest : notnull
       => app.MapPost(template, async (ISender _sender, [AsParameters] IHttpRequest request, CancellationToken cancellationToken) => await _sender.Send(request, cancellationToken));

    public static RouteHandlerBuilder MediatePut<IHttpRequest>(this IEndpointRouteBuilder app, string template) where IHttpRequest : notnull
        => app.MapPut(template, async (ISender _sender, [AsParameters] IHttpRequest request, CancellationToken cancellationToken) => await _sender.Send(request, cancellationToken));

    public static RouteHandlerBuilder MediateDelete<IHttpRequest>(this IEndpointRouteBuilder app, string template) where IHttpRequest : notnull
        => app.MapDelete(template, async (ISender _sender, [AsParameters] IHttpRequest request, CancellationToken cancellationToken) => await _sender.Send(request, cancellationToken));

    public static RouteHandlerBuilder MediatePatch<IHttpRequest>(this IEndpointRouteBuilder app, string template) where IHttpRequest : notnull
        => app.MapPatch(template, async (ISender _sender, [AsParameters] IHttpRequest request, CancellationToken cancellationToken) => await _sender.Send(request, cancellationToken));
}