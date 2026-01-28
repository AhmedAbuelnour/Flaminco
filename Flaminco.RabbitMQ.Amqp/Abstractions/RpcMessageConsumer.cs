namespace Flaminco.RabbitMQ.AMQP.Abstractions;

/// <summary>
/// Base class for RPC message consumers that handle requests and send responses.
/// </summary>
/// <typeparam name="TRequest">The request message type.</typeparam>
/// <typeparam name="TResponse">The response message type.</typeparam>
public abstract class RpcMessageConsumer<TRequest, TResponse>
{
    /// <summary>
    /// Handles an RPC request and returns a response.
    /// </summary>
    /// <param name="context">The RPC context containing the request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response to send back to the caller.</returns>
    public abstract Task<TResponse> HandleAsync(
        RpcContext<TRequest> context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Called when an error occurs during request processing.
    /// Override to customize error handling behavior.
    /// </summary>
    /// <param name="context">The RPC context.</param>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Error handling result.</returns>
    public virtual Task<ErrorHandlingResult> OnErrorAsync(
        RpcContext<TRequest> context,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        // Default: reject the message (sends to DLQ if configured)
        return Task.FromResult(ErrorHandlingResult.Reject);
    }
}
