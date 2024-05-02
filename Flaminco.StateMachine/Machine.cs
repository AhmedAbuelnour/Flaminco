using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.StateMachine
{
    public class Machine(IServiceProvider serviceProvider) : IMachine
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly Dictionary<string, State> States = [];

        private State? StartState { get; set; }
        private State? StopState { get; set; }
        private State? CurrentState { get; set; }
        private State CreateState(string identifier) => new(identifier, this);

        public string? GetCurrentStateIdentifier => CurrentState?.Identifier;

        public async ValueTask SetCurrentAsync(State? state, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(state);

            foreach (ITransition transition in GetTransitions(state.BeforeTransitions).OrderBy(a => a.Order))
            {
                await transition.ExecuteAsync(this, cancellationToken);
            }

            CurrentState = state;

            foreach (ITransition transition in GetTransitions(state.AfterTransitions).OrderBy(a => a.Order))
            {
                await transition.ExecuteAsync(this, cancellationToken);
            }
        }

        public State NewState(string identifier)
        {
            if (States.ContainsKey(identifier))
            {
                throw new ArgumentException("Duplicated state identifier", nameof(identifier));
            }

            State? newState = CreateState(identifier);

            States[identifier] = newState;

            StartState ??= newState;

            return newState;
        }

        public State NewStartState(string identifier) => StartState = NewState(identifier);

        public State NewStopState(string identifier) => StopState = NewState(identifier);

        public State GetState(string identifier)
        {
            if (States.TryGetValue(identifier, out State? state))
            {
                return state;
            }
            else
            {
                throw new ArgumentException("Unknown identifier", identifier.ToString());
            }
        }


        public ValueTask StartAsync(CancellationToken cancellationToken = default) => SetCurrentAsync(StartState, cancellationToken);

        public ValueTask StopAsync(CancellationToken cancellationToken = default) => SetCurrentAsync(StopState, cancellationToken);


        private IEnumerable<ITransition> GetTransitions(List<Type> transitionTypes)
        {
            var xx = _serviceProvider.GetServices<ITransition>();

            foreach (var transitionType in transitionTypes)
            {
                if (_serviceProvider.GetServices<ITransition>()?.FirstOrDefault(a => a.GetType() == transitionType) is ITransition transition)
                {
                    yield return transition;
                }
            }
        }
    }
}
