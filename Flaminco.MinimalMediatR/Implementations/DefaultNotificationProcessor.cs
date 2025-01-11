using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Flaminco.MinimalMediatR.Implementations
{
    internal sealed class DefaultNotificationProcessor(InMemoryMessageQueue queue,
                                                       IServiceScopeFactory _serviceScopeFactory,
                                                       ILogger<DefaultNotificationProcessor> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            await foreach (INotification notification in queue.Reader.ReadAllAsync(stoppingToken))
            {
                using IServiceScope scope = _serviceScopeFactory.CreateScope();

                try
                {
                    IPublisher publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

                    await publisher.Publish(notification, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error in error handler for {NotificationType}", notification.GetType().Name);
                }
            }
        }
    }
}
