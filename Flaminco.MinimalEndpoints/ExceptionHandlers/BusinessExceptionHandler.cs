using Flaminco.MinimalEndpoints.Exceptions;
using Flaminco.MinimalEndpoints.Options;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Flaminco.MinimalEndpoints.ExceptionHandlers
{
    /// <summary>
    /// Handles exceptions of type <see cref="BusinessException"/> and converts them into a standardized <see cref="ProblemDetails"/> response.
    /// </summary>
    internal sealed class BusinessExceptionHandler(IProblemDetailsService _problemDetailsService,
                                                  IExceptionHandlerOptions<BusinessException> validationExceptionHandlerOptions,
                                                  ILogger<BusinessExceptionHandler> _logger,
                                                  IStringLocalizer? stringLocalizer = default) : IExceptionHandler
    {
        /// <summary>
        /// Attempts to handle the provided exception if it is of type <see cref="BusinessException"/>.
        /// </summary>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <param name="exception">The exception to handle.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="ValueTask{TResult}"/> indicating whether the exception was handled.</returns>
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is BusinessException businessException)
            {
                ProblemDetails problemDetails = ToProblem(httpContext, businessException);

                _logger.LogError(exception, "Business exception occurred: {ProblemDetails}", JsonSerializer.Serialize(problemDetails, JsonSerializerOptions.Web));

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
        /// Converts a <see cref="BusinessException"/> into a <see cref="ProblemDetails"/> object.
        /// </summary>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <param name="businessException">The business exception to convert.</param>
        /// <returns>A <see cref="ProblemDetails"/> object representing the exception.</returns>
        private ProblemDetails ToProblem(HttpContext httpContext, BusinessException businessException)
        {
            httpContext.Response.StatusCode = businessException.StatusCode; // Explicitly set the status code

            return new ProblemDetails
            {
                Type = $"{validationExceptionHandlerOptions.Type}{businessException.ErrorCode}",
                Status = businessException.StatusCode,
                Title = validationExceptionHandlerOptions.Title,
                Detail = stringLocalizer?[businessException.ErrorCode] ?? businessException.Details,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
                Extensions = new Dictionary<string, object?>
                    {
                        { "errorCode" , businessException.ErrorCode },
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
            string? userIdentifier = httpContext.User.Claims.FirstOrDefault(claim => claim.Type == validationExceptionHandlerOptions.UserIdentifierTokenName)?.Value;

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
