namespace Flaminco.StateMachine
{
    public class State(string identifier, Machine machine)
    {
        internal string Identifier = identifier;

        internal Machine Machine = machine;

        internal readonly List<Type> BeforeTransitions = [];

        internal readonly List<Type> AfterTransitions = [];

        public void OnEntry<Transition>() where Transition : ITransition
            => BeforeTransitions.Add(typeof(Transition));

        public void OnExit<Transition>() where Transition : ITransition
            => AfterTransitions.Add(typeof(Transition));

        public ValueTask EnterAsync(CancellationToken cancellationToken = default) => Machine.SetCurrentAsync(this, cancellationToken);
    }
}
