using Flaminco.MinimalMediatR.Options;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Flaminco.MinimalMediatR.Exceptions
{
    internal sealed class ValidationsExceptionHandler(IProblemDetailsService _problemDetailsService,
                                                      IExceptionHandlerOptions<ValidationException> validationExceptionHandlerOptions,
                                                      ILogger<ValidationsExceptionHandler> _logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is ValidationException validationException)
            {
                _logger.LogError("Validation exception occurred: {ValidationErrors}", validationException.Errors);

                await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
                {
                    HttpContext = httpContext,
                    Exception = exception,
                    ProblemDetails = ToProblem(httpContext, validationException),
                });

                return true;
            }

            return false;
        }

        private ProblemDetails ToProblem(HttpContext httpContext, ValidationException validationException)
        {
            // Aggregate multiple validation errors into a dictionary format

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest; // Explicitly set the status code

            return new ProblemDetails
            {
                Type = validationExceptionHandlerOptions.Type,
                Status = StatusCodes.Status400BadRequest,
                Title = validationExceptionHandlerOptions.Title,
                Detail = "One or more validation errors occurred. Please refer to the 'errors' field for details.",
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
                Extensions = new Dictionary<string, object?>
                {
                    {
                        "errors", validationException.Errors.Select(failure => new
                        {
                            failure.ErrorCode,
                            failure.ErrorMessage,
                            failure.PropertyName,
                            failure.AttemptedValue,
                        })
                    },  // Add the aggregated errors dictionary
                    { "traceId", $"{httpContext.Features.Get<IHttpActivityFeature>()?.Activity?.Id}" },
                    { "timestamp", DateTime.UtcNow.ToString("o") },
                    { "userId", GetUserId(httpContext) },
                    { "userAgent", httpContext.Request.Headers.UserAgent.ToString() },
                    { "queryParams", httpContext.Request.Query.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString()) }
                }
            };
        }

        private string? GetUserId(HttpContext httpContext)
        {
            // Check for the user ID in claims
            string? userId = httpContext.User.Claims.FirstOrDefault(claim => claim.Type == validationExceptionHandlerOptions.UserIdTokenName)?.Value;

            // If not found in claims, check headers
            if (string.IsNullOrEmpty(userId))
            {
                if (httpContext.Request.Headers.TryGetValue(validationExceptionHandlerOptions.UserIdTokenName, out var headerValue))
                {
                    userId = headerValue.ToString();
                }
                else
                {
                    return httpContext.User.Identity?.IsAuthenticated == true ? httpContext.User.Identity.Name : null;
                }
            }

            return userId;
        }
    }
}
