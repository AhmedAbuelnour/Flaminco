namespace Flaminco.StateMachine
{
    public interface ITransition
    {
        int Order { get; set; }
        ValueTask ExecuteAsync(Machine machine, CancellationToken cancellationToken = default);
    }

    public class EntryTransition : ITransition
    {
        public int Order { get; set; } = 1;
        public async ValueTask ExecuteAsync(Machine machine, CancellationToken cancellationToken = default)
        {

        }
    }

    public class ExitTransition : ITransition
    {
        public int Order { get; set; } = 2;

        public async ValueTask ExecuteAsync(Machine machine, CancellationToken cancellationToken = default)
        {


            Console.WriteLine("Exit");
        }
    }
}
