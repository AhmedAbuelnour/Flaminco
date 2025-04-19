using Flaminco.MinimalEndpoints.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Flaminco.MinimalEndpoints.Implementations
{
    internal sealed class EventProcessor(EventBus eventBus, IServiceScopeFactory serviceScopeFactory) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (IDomainEvent domainEvent in eventBus.Reader.ReadAllAsync(stoppingToken))
            {
                await using AsyncServiceScope scope = serviceScopeFactory.CreateAsyncScope();

                if (scope.ServiceProvider.GetServices(typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType())) is IEnumerable<object> handlers)
                {
                    foreach (object handler in handlers!)
                    {
                        if (handler.GetType().GetMethod("Handle") is MethodInfo methodInfo)
                        {
                            if (methodInfo.Invoke(handler, [domainEvent, stoppingToken]) is ValueTask valueTask)
                            {
                                await valueTask;
                            }
                        }
                    }

                }
            }
        }
    }
}
