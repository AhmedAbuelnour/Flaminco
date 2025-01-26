namespace Flaminco.StateMachine
{
    public record class StateSnapshot(string Key, string Previous, string Current, long Timestamp);
}
