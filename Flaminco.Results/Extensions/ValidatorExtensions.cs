using Flaminco.Results.Implementations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Flaminco.Results.Extensions;

public static class ValidatorExtensions
{
    public static RouteHandlerBuilder AddValidator<TValue>(this RouteHandlerBuilder handlerBuilder) where TValue : class
    {
        handlerBuilder.AddEndpointFilter<ValidatorEndpointFilter<TValue>>();

        return handlerBuilder;
    }
}
