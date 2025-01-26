namespace Flaminco.StateMachine
{
    public sealed class StateContext<TObject>(State<TObject> InitialState, TObject Object) where TObject : notnull, new()
    {
        private State<TObject> _currentState = InitialState;

        public TObject Object { get; set; } = Object;

        public IList<StateSnapshot> StateSnapshots { get; } = [];

        internal void SetState(State<TObject> state) => _currentState = state;

        public async ValueTask ProcessStateMachineAsync(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested && await _currentState.ExecuteAsync(this, cancellationToken)) ;
        }
    }
}
