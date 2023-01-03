namespace Flaminco.StateMachine.Abstractions
{
    public interface IStateContext
    {
        ValueTask Execute(IState? start,ISharedValue? sharedValue = default, CancellationToken cancellationToken = default);
    }
}
