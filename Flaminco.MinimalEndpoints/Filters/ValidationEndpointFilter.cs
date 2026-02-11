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
                    IReadOnlyList<ValidationFailure> failures = validator.Validate(request).ToList();

                    if (failures.Any())
                    {
                        Dictionary<string, string[]> errors = failures
                            .GroupBy(f => string.IsNullOrWhiteSpace(f.PropertyName) ? "general" : f.PropertyName)
                            .ToDictionary(
                                g => g.Key,
                                g => g.Select(x => x.ErrorMessage ?? "Validation failed").ToArray()
                            );

                        return Results.ValidationProblem(errors: errors,
                                                         title: "Validation Error",
                                                         instance: context.HttpContext.Request.Path,
                                                         type: "https://httpstatuses.com/400",
                                                         statusCode: StatusCodes.Status400BadRequest,
                                                         extensions: new Dictionary<string, object?>
                        {
                            { "timestamp", DateTimeOffset.UtcNow.ToString("O") },
                            { "traceId", context.HttpContext.TraceIdentifier },
                            { "failures", failures }
                        });
                    }

                }
                else if (context.HttpContext.RequestServices.GetService<IAsyncMinimalValidator<TRequest>>() is IAsyncMinimalValidator<TRequest> asyncValidator)
                {
                    List<ValidationFailure> failures = [];

                    await foreach (ValidationFailure failure in asyncValidator.ValidateAsync(request, context.HttpContext.RequestAborted))
                    {
                        failures.Add(failure);
                    }

                    if (failures.Count != 0)
                    {
                        Dictionary<string, string[]> errors = failures
                              .GroupBy(f => string.IsNullOrWhiteSpace(f.PropertyName) ? "general" : f.PropertyName)
                              .ToDictionary(
                                  g => g.Key,
                                  g => g.Select(x => x.ErrorMessage ?? "Validation failed").ToArray()
                              );

                        return Results.ValidationProblem(errors: errors,
                                                         title: "Validation Error",
                                                         instance: context.HttpContext.Request.Path,
                                                         type: "https://httpstatuses.com/400",
                                                         statusCode: StatusCodes.Status400BadRequest,
                                                         extensions: new Dictionary<string, object?>
                        {
                            { "timestamp", DateTimeOffset.UtcNow.ToString("O") },
                            { "traceId", context.HttpContext.TraceIdentifier },
                            { "failures", failures }
                        });
                    }
                }
            }

            return await next(context);
        }
    }
}
