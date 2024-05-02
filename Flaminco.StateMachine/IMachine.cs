
namespace Flaminco.StateMachine
{
    public interface IMachine
    {
        string? GetCurrentStateIdentifier { get; }
        State GetState(string identifier);
        State NewStartState(string identifier);
        State NewState(string identifier);
        State NewStopState(string identifier);
        ValueTask SetCurrentAsync(State? state, CancellationToken cancellationToken = default);
        ValueTask StartAsync(CancellationToken cancellationToken = default);
        ValueTask StopAsync(CancellationToken cancellationToken = default);
    }
}