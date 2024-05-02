namespace Flaminco.StateMachine
{
    public class MachineOptions
    {
        public List<StateOptions> StateOptions { get; set; }
    }

    public class StateOptions
    {
        public State State { get; set; }
        public StateStage Stage { get; set; } = StateStage.Normal;
    }


    public enum StateStage
    {
        Normal,
        Start,
        End,
    }
}
