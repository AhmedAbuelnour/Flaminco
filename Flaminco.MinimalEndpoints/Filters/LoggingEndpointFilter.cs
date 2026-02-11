using Flaminco.MinimalEndpoints.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Flaminco.MinimalEndpoints.Filters
{
    /// <summary>
    /// An endpoint filter that provides logging functionality for minimal endpoints.
    /// </summary>
    /// <typeparam name="TEndpoint">The type of endpoint being filtered</typeparam>
    /// <param name="logger">The logger instance for the endpoint</param>
    internal sealed class LoggingEndpointFilter<TEndpoint>(ILogger<TEndpoint> logger) : IEndpointFilter where TEndpoint : IMinimalEndpoint
    {
        /// <summary>
        /// Invokes the filter to log the start and end of endpoint execution along with timing information.
        /// </summary>
        /// <param name="context">The endpoint filter invocation context</param>
        /// <param name="next">The next filter in the pipeline</param>
        /// <returns>A value task containing the result of the endpoint execution</returns>
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            long startTimestamp = Stopwatch.GetTimestamp();

            logger.LogInformation("Handling endpoint {EndpointName}, at {StartTime} UTC", typeof(TEndpoint).Name, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            object? result = await next(context);

            logger.LogInformation("Handled endpoint {EndpointName} in {ElapsedMilliseconds} ms, at {EndTime} UTC",
                                  typeof(TEndpoint).Name, Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds,
                                  DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            return result;
        }
    }
}
