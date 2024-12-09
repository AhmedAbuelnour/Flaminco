using Flaminco.MinimalMediatR.Abstractions;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Flaminco.MinimalMediatR.Implementations
{
    internal sealed class DefaultNotificationProcessor(InMemoryMessageQueue queue,
                                                       IPublisher publisher,
                                                       INotificationErrorHandler notificationErrorHandler,
                                                       ILogger<DefaultNotificationProcessor> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (INotification notification in queue.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    await publisher.Publish(notification, stoppingToken);
                }
                catch (Exception ex)
                {
                    if (!await notificationErrorHandler.HandleAsync(notification, ex, stoppingToken))
                    {
                        // If the error handler decides not to handle, log and continue
                        logger.LogError(ex, "Unhandled error processing notification: {Notification}", notification);
                    }
                }
            }

        }
    }
}
