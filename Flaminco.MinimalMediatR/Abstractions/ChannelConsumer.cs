using MediatR;

namespace Flaminco.MinimalMediatR.Abstractions
{
    public abstract class ChannelConsumer<TNotification> : INotificationHandler<TNotification>, INotificationErrorHandler<TNotification> where TNotification : INotification
    {
        public async Task Handle(TNotification notification, CancellationToken cancellationToken)
        {
            try
            {
                await Consume(notification, cancellationToken);
            }
            catch (Exception ex)
            {
                await Consume(notification, ex, cancellationToken);
            }
        }

        /// <summary>
        /// Handles the notification of type <typeparamref name="TNotification" />.
        /// </summary>
        /// <param name="notification">The notification to handle.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public abstract Task Consume(TNotification notification, CancellationToken cancellationToken);

        /// <summary>
        /// Handles errors that occur during notification processing.
        /// </summary>
        /// <param name="notification">The notification being processed.</param>
        /// <param name="exception">The exception that occurred.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public virtual Task Consume(TNotification notification, Exception exception, CancellationToken cancellationToken)
        {
            // Default implementation for error handling (can be overridden in derived classes)
            Console.WriteLine($"Error occurred while processing {typeof(TNotification).Name}: {exception.Message}");

            return Task.CompletedTask;
        }
    }
}
