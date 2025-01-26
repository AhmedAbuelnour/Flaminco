using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace Flaminco.StateMachine
{
    public abstract class State<TObject>(ILogger _logger) where TObject : notnull, new()
    {
        public abstract string Key { get; }

        /// <summary>
        /// Returns true if the a state transition is expected and the context should carry on executing, false if you want to get out of the state machine.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async ValueTask<bool> ExecuteAsync(StateContext<TObject> context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Capture object state before execution
                string previous = JsonSerializer.Serialize(context.Object, JsonSerializerOptions.Web);

                _logger.LogInformation("Entering state: {StateKey}", Key);

                bool shouldContinue = await Handle(context, cancellationToken);

                _logger.LogInformation("Exiting state: {StateKey}", Key);

                string current = JsonSerializer.Serialize(context.Object, JsonSerializerOptions.Web);

                context.StateSnapshots.Add(new StateSnapshot(Key, previous, current, Stopwatch.GetTimestamp()));

                return shouldContinue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in state: {StateKey}", Key);

                return await Handle(context, ex, cancellationToken);
            }
        }

        public abstract ValueTask<bool> Handle(StateContext<TObject> context, CancellationToken cancellationToken = default);
        public abstract ValueTask<bool> Handle(StateContext<TObject> context, Exception exception, CancellationToken cancellationToken = default);
    }
}
