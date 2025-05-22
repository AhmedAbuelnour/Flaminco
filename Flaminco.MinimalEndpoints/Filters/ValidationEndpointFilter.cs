using Flaminco.MinimalEndpoints.Abstractions;
using Flaminco.MinimalEndpoints.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
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
                        return Results.BadRequest(ToProblem(context.HttpContext, failures));
                }
                else if (context.HttpContext.RequestServices.GetService<IAsyncMinimalValidator<TRequest>>() is IAsyncMinimalValidator<TRequest> asyncValidator)
                {
                    IEnumerable<ValidationFailure> failures = await asyncValidator.ValidateAsync(request, context.HttpContext.RequestAborted);

                    if (failures.Any())
                        return Results.BadRequest(ToProblem(context.HttpContext, failures));
                }
            }

            return await next(context);
        }

        /// <summary>
        /// Converts a <see cref="ValidationFailure"/> into a <see cref="ProblemDetails"/> object.
        /// </summary>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <param name="validationFailures">The validation failures to convert.</param>
        /// <returns>A <see cref="ProblemDetails"/> object representing the validation errors.</returns>
        private ProblemDetails ToProblem(HttpContext httpContext, IEnumerable<ValidationFailure> validationFailures)
        {
            // Aggregate multiple validation errors into a dictionary format

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest; // Explicitly set the status code

            return new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = "One or more validation errors occurred. Please refer to the 'errors' field for details.",
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
                Extensions = new Dictionary<string, object?>
                {
                    {
                        "errors", validationFailures.Select(failure => new
                        {
                            failure.ErrorCode,
                            failure.ErrorMessage,
                            failure.PropertyName,
                            failure.AttemptedValue,
                        })
                    },  // Add the aggregated errors dictionary
                    { "traceId", $"{httpContext.Features.Get<IHttpActivityFeature>()?.Activity?.Id}" },
                    { "timestamp", DateTime.UtcNow.ToString("o") },
                    { "userAgent", httpContext.Request.Headers.UserAgent.ToString() },
                }
            };
        }

    }
}
