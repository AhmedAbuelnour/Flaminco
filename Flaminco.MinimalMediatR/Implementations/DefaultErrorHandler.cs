using Flaminco.MinimalMediatR.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Flaminco.MinimalMediatR.Implementations
{

    public class DefaultErrorHandler(ILogger<DefaultErrorHandler> logger) : INotificationErrorHandler
    {
        public ValueTask<bool> HandleAsync(INotification notification, Exception exception, CancellationToken cancellationToken)
        {
            // Log the error and mark it as handled
            logger.LogError(exception, "Error processing notification: {Notification}", notification);

            // Return true to indicate the error was handled

            return ValueTask.FromResult(true);
        }
    }

}
