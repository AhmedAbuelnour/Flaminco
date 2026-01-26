namespace Flaminco.RabbitMQ.AMQP.Abstractions;

/// <summary>
/// Abstract base class for consuming messages from RabbitMQ queues.
/// Inherit from this class and decorate with [Queue] attribute to create a consumer.
/// </summary>
/// <typeparam name="TMessage">The type of message to consume.</typeparam>
public abstract class MessageConsumer<TMessage>
{
    /// <summary>
    /// Gets the message type that this consumer handles.
    /// </summary>
    public Type MessageType => typeof(TMessage);

    /// <summary>
    /// Processes a received message. Override this method to implement your message handling logic.
    /// </summary>
    /// <param name="context">The message context containing the message and metadata.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public abstract Task ConsumeAsync(ConsumeContext<TMessage> context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Called when an error occurs during message consumption.
    /// Override this method to implement custom error handling.
    /// </summary>
    /// <param name="context">The message context.</param>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An error handling result indicating what action to take.</returns>
    public virtual Task<ErrorHandlingResult> OnErrorAsync(
        ConsumeContext<TMessage> context,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        // Default behavior: reject and do not requeue
        return Task.FromResult(ErrorHandlingResult.Reject);
    }

    /// <summary>
    /// Called before the message is processed.
    /// Override this method to add pre-processing logic (e.g., logging, validation).
    /// </summary>
    /// <param name="context">The message context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True to continue processing, false to skip and acknowledge.</returns>
    public virtual Task<bool> OnBeforeConsumeAsync(
        ConsumeContext<TMessage> context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    /// <summary>
    /// Called after the message is successfully processed.
    /// Override this method to add post-processing logic.
    /// </summary>
    /// <param name="context">The message context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public virtual Task OnAfterConsumeAsync(
        ConsumeContext<TMessage> context,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

/// <summary>
/// Specifies the result of error handling.
/// </summary>
public enum ErrorHandlingResult
{
    /// <summary>
    /// Acknowledge the message (remove from queue).
    /// </summary>
    Acknowledge,

    /// <summary>
    /// Reject the message without requeuing. Goes to DLQ if configured.
    /// </summary>
    Reject,

    /// <summary>
    /// Reject the message and requeue it for retry.
    /// </summary>
    Requeue
}
