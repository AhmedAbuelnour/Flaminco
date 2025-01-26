namespace Flaminco.StateMachine
{

    /// <summary>
    /// Represents a snapshot of a state transition.
    /// </summary>
    /// <param name="Key">The unique key identifying the state.</param>
    /// <param name="Previous">The previous state value.</param>
    /// <param name="Current">The current state value.</param>
    /// <param name="Timestamp">The timestamp of the state transition.</param>
    public sealed record class StateSnapshot(string Key, string Previous, string Current, long Timestamp);
}
