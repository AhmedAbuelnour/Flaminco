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
    internal sealed class BusinessExceptionHandler(IProblemDetailsService _problemDetailsService,
                                                  IExceptionHandlerOptions<BusinessException> validationExceptionHandlerOptions,
                                                  ILogger<ValidationsExceptionHandler> _logger,
                                                  IStringLocalizer? stringLocalizer = default) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is BusinessException businessException)
            {
                ProblemDetails problemDetails = ToProblem(httpContext, businessException);

                _logger.LogError(exception, "Business exception occurred: {problemDetails}", JsonSerializer.Serialize(problemDetails, JsonSerializerOptions.Default));

                return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
                {
                    HttpContext = httpContext,
                    Exception = exception,
                    ProblemDetails = problemDetails,
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
