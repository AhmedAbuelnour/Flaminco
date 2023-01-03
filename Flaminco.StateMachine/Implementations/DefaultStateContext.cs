using Flaminco.StateMachine.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.StateMachine.Implementations;

public class DefaultStateContext : IStateContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly List<IState> States;
    public DefaultStateContext(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        States = new List<IState>();
    }

    public async ValueTask Execute(IState? start, ISharedValue? sharedValue = default, Action<IState>? onTransition = default, Action<IEnumerable<IState>>? onComplete = default, CancellationToken cancellationToken = default)
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
        else
        {
            States.Add(start);

            onTransition?.Invoke(state!);

            IState? nextState = await state!.Handle(sharedValue, cancellationToken);

            if (state?.IsCircuitBreakerState == false)
            {
                await Execute(nextState, sharedValue, onTransition, onComplete, cancellationToken);
            }
            else
            {
                onComplete?.Invoke(States);

                await ValueTask.CompletedTask;
            }
        }
    }
}