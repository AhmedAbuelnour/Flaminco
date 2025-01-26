using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace Flaminco.StateMachine
{
    /// <summary>
    /// Represents an abstract state in the state machine.
    /// </summary>
    /// <typeparam name="TPayload">The type of the object associated with the state.</typeparam>
    public abstract class State<TPayload>(ILogger _logger) where TPayload : notnull, new()
    {
        /// <summary>
        /// Gets the key of the state by reading the [StateKey] attribute.
        /// </summary>
        protected string Key => GetType().GetCustomAttribute<StateKeyAttribute>()?.Key ?? throw new InvalidOperationException($"The state '{GetType().Name}' is missing the [StateKey] attribute.");


        /// <summary>
        /// Executes the state logic asynchronously.
        /// </summary>
        /// <param name="context">The state context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether to continue to the next state.</returns>
        public async ValueTask<bool> ExecuteAsync(StateContext<TPayload> context, CancellationToken cancellationToken = default)
        {
            try
            {
                string previous = JsonSerializer.Serialize(context.Payload, JsonSerializerOptions.Web);

                _logger.LogInformation("Entering state: {StateKey}", Key);

                bool shouldContinue = await Handle(context, cancellationToken);

                _logger.LogInformation("Exiting state: {StateKey}", Key);

                string current = JsonSerializer.Serialize(context.Payload, JsonSerializerOptions.Web);

                context.StateSnapshots.Add(new StateSnapshot(Key, previous, current, Stopwatch.GetTimestamp()));

                return shouldContinue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in state: {StateKey}", Key);

                return await Handle(context, ex, cancellationToken);
            }
        }

        /// <summary>
        /// Handles the state logic asynchronously.
        /// </summary>
        /// <param name="context">The state context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether to continue to the next state.</returns>
        public abstract ValueTask<bool> Handle(StateContext<TPayload> context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Handles the state logic asynchronously when an exception occurs.
        /// </summary>
        /// <param name="context">The state context.</param>
        /// <param name="exception">The exception that occurred.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether to continue to the next state.</returns>
        public abstract ValueTask<bool> Handle(StateContext<TPayload> context, Exception exception, CancellationToken cancellationToken = default);
    }
}
