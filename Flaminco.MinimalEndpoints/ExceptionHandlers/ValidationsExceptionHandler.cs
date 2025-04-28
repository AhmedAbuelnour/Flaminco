using Flaminco.MinimalEndpoints.Options;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Flaminco.MinimalEndpoints.ExceptionHandlers
{
    /// <summary>
    /// Handles validation exceptions and converts them into a standardized problem details response.
    /// </summary>
    internal sealed class ValidationsExceptionHandler(IProblemDetailsService _problemDetailsService,
                                                      IExceptionHandlerOptions<ValidationException> validationExceptionHandlerOptions,
                                                      ILogger<ValidationsExceptionHandler> _logger) : IExceptionHandler
    {
        /// <summary>
        /// Attempts to handle the provided exception if it is a <see cref="ValidationException"/>.
        /// </summary>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <param name="exception">The exception to handle.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="ValueTask{TResult}"/> indicating whether the exception was handled.</returns>
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is ValidationException validationException)
            {
                ProblemDetails problemDetails = ToProblem(httpContext, validationException);

                _logger.LogError(exception, "Validation exception occurred: {ProblemDetails}", JsonSerializer.Serialize(problemDetails, JsonSerializerOptions.Default));

                return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
                {
                    HttpContext = httpContext,
                    Exception = exception,
                    ProblemDetails = problemDetails,
                });
            }

            return false;
        }

        /// <summary>
        /// Converts a <see cref="ValidationException"/> into a <see cref="ProblemDetails"/> object.
        /// </summary>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <param name="validationException">The validation exception to convert.</param>
        /// <returns>A <see cref="ProblemDetails"/> object representing the validation errors.</returns>
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
                        { "userIdentifier", GetUserIdentifier(httpContext) },
                        { "userAgent", httpContext.Request.Headers.UserAgent.ToString() },
                        { "queryParams", httpContext.Request.Query.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString()) }
                    }
            };
        }

        /// <summary>
        /// Retrieves the user identifier from the HTTP context.
        /// </summary>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <returns>The user identifier, or <c>null</c> if not found.</returns>
        private string? GetUserIdentifier(HttpContext httpContext)
        {
            // Check for the user ID in claims
            string? userIdentifier = httpContext.User.Claims.FirstOrDefault(claim => claim.Type == validationExceptionHandlerOptions.UserIdentifierTokenName)?.Value;

            // If not found in claims, check headers
            if (string.IsNullOrEmpty(userIdentifier))
            {
                if (httpContext.Request.Headers.TryGetValue(validationExceptionHandlerOptions.UserIdentifierTokenName, out var headerValue))
                {
                    userIdentifier = headerValue.ToString();
                }
                else
                {
                    return httpContext.User.Identity?.IsAuthenticated == true ? httpContext.User.Identity.Name : null;
                }
            }

            return userIdentifier;
        }
    }
}
