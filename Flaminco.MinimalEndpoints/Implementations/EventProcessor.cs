using Flaminco.MinimalEndpoints.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Flaminco.MinimalEndpoints.Implementations
{
    /// <summary>
    /// Represents a background service that processes domain events from the EventBus.
    /// </summary>
    internal sealed class EventProcessor(IEventBus eventBus, IServiceScopeFactory serviceScopeFactory, ILogger<EventProcessor> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var domainEvent in eventBus.ReadAllAsync(stoppingToken))
            {
                try
                {
                    await using var scope = serviceScopeFactory.CreateAsyncScope();

                    var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());

                    var handlers = scope.ServiceProvider.GetServices(handlerType);

                    foreach (var handler in handlers)
                    {
                        var method = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.Handle));

                        if (method is null)
                        {
                            logger.LogWarning("Handler {Handler} does not implement Handle properly.", handler?.GetType().FullName);

                            continue;
                        }

                        if (method.Invoke(handler, [domainEvent, stoppingToken]) is ValueTask valueTask)
                        {
                            await valueTask;
                        }

                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to process domain event {EventType}", domainEvent.GetType().Name);
                    // Optionally add retry, DLQ, or monitoring hooks here
                }
            }
        }
    }
}
