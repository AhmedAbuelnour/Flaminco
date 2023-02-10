namespace Flaminco.StateMachine.Abstractions;

public interface IStateContext
{
    ValueTask Execute(IState? start, ISharedValue? sharedValue = default, Action<IState>? onTransition = default, Action<IEnumerable<IState>>? onComplete = default, CancellationToken cancellationToken = default);
}
