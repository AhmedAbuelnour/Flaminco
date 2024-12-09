using MediatR;

namespace Flaminco.MinimalMediatR.Abstractions
{
    public interface INotificationErrorHandler
    {
        /// <summary>
        /// Handles the error during processing.
        /// </summary>
        /// <param name="notification">The notification being processed.</param>
        /// <param name="exception">The exception that occurred.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>True if the error was handled, false otherwise.</returns>
        ValueTask<bool> HandleAsync(INotification notification, Exception exception, CancellationToken cancellationToken);
    }
}
