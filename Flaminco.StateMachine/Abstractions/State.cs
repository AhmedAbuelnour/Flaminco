namespace Flaminco.StateMachine.Abstractions
{
    public interface ISharedValue
    {

    }

    public interface IState
    {
        bool IsCircuitBreakerState { get; }
        string Name { get; }
        ValueTask<IState?> Handle(ISharedValue? value = default, CancellationToken cancellationToken = default);
    }
}
