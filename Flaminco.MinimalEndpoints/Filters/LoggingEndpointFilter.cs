using Flaminco.MinimalEndpoints.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Flaminco.MinimalEndpoints.Filters
{
    internal sealed class LoggingEndpointFilter<TEndpoint>(ILogger<TEndpoint> logger) : IEndpointFilter where TEndpoint : IMinimalRouteEndpoint
    {
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
