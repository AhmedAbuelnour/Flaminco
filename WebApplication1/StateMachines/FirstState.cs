using Flaminco.StateMachine.Abstractions;

namespace WebApplication1.StateMachines
{
    public class SharedValue : ISharedValue
    {
        public int Value { get; set; }
    }
    public class FirstState : IState
    {
        public string Name => nameof(FirstState);
        public async ValueTask<IState?> Handle(ISharedValue? value = default, CancellationToken cancellationToken = default)
        {
            if (value is SharedValue sharedValue)
            {
                sharedValue.Value = 1;
            }

            return await ValueTask.FromResult(new SecondState());
        }

        public bool IsCircuitBreakerState => false;
    }


    public record SecondState : IState
    {
        public string Name => nameof(SecondState);
        public async  ValueTask<IState?> Handle(ISharedValue? value = default, CancellationToken cancellationToken = default)
        {
            if (value is SharedValue sharedValue)
            {
                sharedValue.Value = 2;
            }

            return await ValueTask.FromResult(new ThirdState());
        }

        public bool IsCircuitBreakerState => false;
    }

    public record ThirdState : IState
    {
        public string Name => nameof(ThirdState);
        public ValueTask<IState?> Handle(ISharedValue? value = default, CancellationToken cancellationToken = default)
        {
            if (value is SharedValue sharedValue)
            {
                sharedValue.Value = 10;
            }
            return default;
        }

        public bool IsCircuitBreakerState => true;
    }
}
