using MediatR;

namespace Flaminco.MinimalMediatR.Abstractions
{
    internal interface INotificationErrorHandler<TNotification> where TNotification : INotification
    {
        /// <summary>
        /// Handles the error during processing.
        /// </summary>
        /// <param name="notification">The notification being processed.</param>
        /// <param name="exception">The exception that occurred.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        Task Consume(TNotification notification, Exception exception, CancellationToken cancellationToken);
    }
}
