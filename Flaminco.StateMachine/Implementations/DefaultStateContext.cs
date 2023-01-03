using Flaminco.StateMachine.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.StateMachine.Implementations;

public class DefaultStateContext : IStateContext
{
    private readonly IServiceProvider _serviceProvider;

    public DefaultStateContext(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async ValueTask Execute(IState? start, ISharedValue? sharedValue = default, CancellationToken cancellationToken = default)
    {
        if (start is null)
        {
            await ValueTask.CompletedTask;
        }

        IState? state = _serviceProvider.GetServices<IState>()?.FirstOrDefault(a => a.Name == start!.Name);

        if (start is null)
        {
            await ValueTask.CompletedTask;
        }

        if (state?.IsCircuitBreakerState == true)
        {
            _ = await state!.Handle(sharedValue, cancellationToken);
            
            await ValueTask.CompletedTask;
        }
        else
        { 
           IState? nextState =  await state!.Handle(sharedValue, cancellationToken);

           await Execute(nextState, sharedValue, cancellationToken);
        }
    }
}