using Flaminco.MinimalMediatR.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Flaminco.MinimalMediatR.Behaviors
{
    internal class LoggingPipelineBehavior<TRequest, IResult>(ILogger<LoggingPipelineBehavior<TRequest, IResult>> _logger) : IPipelineBehavior<TRequest, IResult> where TRequest : IEndPointRequest
    {
        public async Task<IResult> Handle(TRequest request, RequestHandlerDelegate<IResult> next, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling {RequestName}", typeof(TRequest).Name);

            long startTimestamp = Stopwatch.GetTimestamp();

            IResult result = await next();

            _logger.LogInformation("Handled {RequestName} in {ElapsedMilliseconds} ms", typeof(TRequest).Name, Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds);

            return result;
        }
    }
}
