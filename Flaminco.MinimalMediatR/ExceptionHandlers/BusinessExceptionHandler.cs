using Flaminco.MinimalMediatR.Exceptions;
using Flaminco.MinimalMediatR.Options;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Flaminco.MinimalMediatR.ExceptionHandlers
{
    internal sealed class BusinessExceptionHandler(IProblemDetailsService _problemDetailsService,
                                                  IExceptionHandlerOptions<BusinessException> validationExceptionHandlerOptions,
                                                  ILogger<ValidationsExceptionHandler> _logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is BusinessException businessException)
            {
                _logger.LogError(exception, "Business exception occurred: {businessException}", businessException.ToString());

                return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
                {
                    HttpContext = httpContext,
                    Exception = exception,
                    ProblemDetails = ToProblem(httpContext, businessException),
                });
            }

            return false;
        }

        private ProblemDetails ToProblem(HttpContext httpContext, BusinessException businessException)
        {
            // Aggregate multiple validation errors into a dictionary format

            httpContext.Response.StatusCode = businessException.StatusCode; // Explicitly set the status code

            return new ProblemDetails
            {
                Type = $"{validationExceptionHandlerOptions.Type}{businessException.ErrorCode}",
                Status = businessException.StatusCode,
                Title = validationExceptionHandlerOptions.Title,
                Detail = businessException.Details,
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
