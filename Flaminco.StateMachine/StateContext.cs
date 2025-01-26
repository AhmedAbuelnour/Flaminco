using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.StateMachine
{

    /// <summary>
    /// Represents the context for the state machine, holding the current state and the object being processed.
    /// </summary>
    /// <typeparam name="TPayload">The type of the object being processed by the state machine.</typeparam>
    /// <param name="serviceProvider">The service provider for creating scopes and resolving services.</param>
    public sealed class StateContext<TPayload>(IServiceProvider serviceProvider) where TPayload : notnull, new()
    {
        private State<TPayload>? _currentState;

        /// <summary>
        /// Gets or sets the object being processed by the state machine.
        /// </summary>
        public TPayload? Payload { get; set; }

        /// <summary>
        /// Gets the list of state snapshots.
        /// </summary>
        public IList<StateSnapshot> StateSnapshots { get; } = [];

        /// <summary>
        /// Sets the current state of the state machine and assigns the object being processed.
        /// </summary>
        /// <param name="key">The key of the new state to set.</param>
        /// <param name="payload">The payload being processed by the state machine.</param>
        public void SetState(string key, TPayload payload)
        {
            Payload = payload;

            SetState(key);
        }

        /// <summary>
        /// Sets the current state of the state machine.
        /// </summary>
        /// <param name="key">The key of the new state to set.</param>
        public void SetState(string key)
        {
            if (serviceProvider.GetKeyedService<State<TPayload>>(key) is State<TPayload> state)
            {
                _currentState = state;
            }
            else
            {
                throw new InvalidOperationException($"The state '{key}' is not registered.");
            }
        }

        /// <summary>
        /// Processes the state machine asynchronously until cancellation is requested.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        public async ValueTask ProcessAsync(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested
                && _currentState is not null
                && await _currentState.ExecuteAsync(this, cancellationToken)) ;
        }
    }
}
