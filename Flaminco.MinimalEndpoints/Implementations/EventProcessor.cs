using Flaminco.MinimalEndpoints.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Flaminco.MinimalEndpoints.Implementations
{
    /// <summary>
    /// Represents a background service that processes domain events from the EventBus.
    /// </summary>
    internal sealed class EventProcessor(EventBus eventBus, IServiceScopeFactory serviceScopeFactory) : BackgroundService
    {
        /// <summary>
        /// Executes the background service to process domain events.
        /// </summary>
        /// <param name="stoppingToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (IDomainEvent domainEvent in eventBus.Reader.ReadAllAsync(stoppingToken))
            {
                AsyncServiceScope scope = serviceScopeFactory.CreateAsyncScope();

                if (scope.ServiceProvider.GetServices(typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType())) is IEnumerable<object> handlers)
                {
                    foreach (object handler in handlers!)
                    {
                        if (handler.GetType().GetMethod("Handle") is MethodInfo methodInfo && methodInfo.Invoke(handler, [domainEvent, stoppingToken]) is ValueTask valueTask)
                        {
                            await valueTask;
                        }
                    }
                }
            }
        }
    }
}
