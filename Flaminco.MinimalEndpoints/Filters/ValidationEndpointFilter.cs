using Flaminco.MinimalEndpoints.Abstractions;
using Flaminco.MinimalEndpoints.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.MinimalEndpoints.Filters
{
    public sealed class ValidationEndpointFilter<TRequest> : IEndpointFilter where TRequest : notnull
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            if (context.Arguments.OfType<TRequest>().FirstOrDefault() is TRequest request)
            {
                if (context.HttpContext.RequestServices.GetService<IMinimalValidator<TRequest>>() is IMinimalValidator<TRequest> validator)
                {
                    IEnumerable<ValidationFailure> failures = validator.Validate(request).ToList();

                    if (failures.Any())
                        return TypedResults.BadRequest(EndpointResult<IEnumerable<ValidationFailure>>.Failure("ValidationError", "One or more validation errors occurred.", failures));
                }
                else if (context.HttpContext.RequestServices.GetService<IAsyncMinimalValidator<TRequest>>() is IAsyncMinimalValidator<TRequest> asyncValidator)
                {
                    IEnumerable<ValidationFailure> failures = await asyncValidator.ValidateAsync(request, context.HttpContext.RequestAborted);

                    if (failures.Any())
                        return TypedResults.BadRequest(EndpointResult<IEnumerable<ValidationFailure>>.Failure("ValidationError", "One or more validation errors occurred.", failures));
                }
            }

            return await next(context);
        }
    }
}
