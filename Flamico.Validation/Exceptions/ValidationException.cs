using ErrorOr;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace Flaminco.Validation.Exceptions
{
    public class ProblemException : Exception
    {
        public Error Error { get; set; }
    }

    public class ValidationException(string type = "Bad Request", string title = "Validation failed", string description = "One or more validation errors occurred") : Exception
    {
        public List<Error> Errors { get; set; }
    }


    public class ProblemExceptionHandler(IProblemDetailsService _problemDetailsService) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is ProblemException problemException)
            {
                await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
                {
                    HttpContext = httpContext,
                    ProblemDetails = ToProblem(httpContext, problemException),
                });

                return true;
            }

            return false;
        }


        private static ProblemDetails ToProblem(HttpContext httpContext, ProblemException problemException)
        {
            (int statusCode, string type) = problemException.Error.Type switch
            {
                ErrorType.Validation => (StatusCodes.Status400BadRequest, "Bad Request"),
                ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                ErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden"),
                ErrorType.NotFound => (StatusCodes.Status404NotFound, "Not Found"),
                ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
                ErrorType.Failure => (StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"),
                ErrorType.Unexpected => (StatusCodes.Status500InternalServerError, "Internal Server Error"),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
            };

            httpContext.Response.StatusCode = statusCode; // Explicitly set the status code

            return new ProblemDetails
            {
                Type = type,
                Status = statusCode,
                Title = problemException.Error.Code,
                Detail = problemException.Error.Description,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
                Extensions = new Dictionary<string, object?>
                {
                    { "traceId", $"{httpContext.Features.Get<IHttpActivityFeature>()?.Activity?.Id}" },
                    { "timestamp", DateTime.UtcNow.ToString("o") },
                    { "userId", httpContext.User.Identity?.IsAuthenticated == true ? httpContext.User.Identity.Name : null },
                    { "userAgent", httpContext.Request.Headers.UserAgent.ToString() },
                    { "queryParams", httpContext.Request.Query.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString()) }
                }
            };
        }
    }

    public class ValidationsExceptionHandler(IProblemDetailsService _problemDetailsService) : IExceptionHandler
    {

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is ValidationException validationException)
            {
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

        private static ProblemDetails ToProblem(HttpContext httpContext, ValidationException validationException)
        {
            // Aggregate multiple validation errors into a dictionary format
            var errors = validationException.Errors
                .Where(a => a.Type == ErrorType.Validation)
                .GroupBy(e => e.Code)  // Group errors by code or field
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(e => e.Description).ToList()
                );

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest; // Explicitly set the status code

            return new ProblemDetails
            {
                Type = "Bad Request",
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Detail = "One or more validation errors occurred",
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
                Extensions = new Dictionary<string, object?>
                    {
                        { "errors", errors },  // Add the aggregated errors dictionary
                        { "traceId", $"{httpContext.Features.Get<IHttpActivityFeature>()?.Activity?.Id}" },
                        { "timestamp", DateTime.UtcNow.ToString("o") },
                        { "userId", httpContext.User.Identity?.IsAuthenticated == true ? httpContext.User.Identity.Name : null },
                        { "userAgent", httpContext.Request.Headers.UserAgent.ToString() },
                        { "queryParams", httpContext.Request.Query.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString()) }
                    }
            };

        }

    }
}
