namespace Flaminco.RabbitMQ.AMQP.Abstractions;

/// <summary>
/// Client for making RPC (Request-Reply) calls over RabbitMQ.
/// </summary>
public interface IMessageRpcClient
{
    /// <summary>
    /// Makes an RPC call and waits for a response.
    /// </summary>
    /// <typeparam name="TRequest">The request message type.</typeparam>
    /// <typeparam name="TResponse">The response message type.</typeparam>
    /// <param name="exchange">The target exchange name.</param>
    /// <param name="routingKey">The routing key.</param>
    /// <param name="request">The request message.</param>
    /// <param name="timeoutMs">Timeout in milliseconds. Default is 30000 (30 seconds).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response message.</returns>
    /// <exception cref="TimeoutException">Thrown when the RPC call times out.</exception>
    Task<TResponse> CallAsync<TRequest, TResponse>(
        string exchange,
        string routingKey,
        TRequest request,
        int timeoutMs = 30000,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Makes an RPC call with custom options and waits for a response.
    /// </summary>
    /// <typeparam name="TRequest">The request message type.</typeparam>
    /// <typeparam name="TResponse">The response message type.</typeparam>
    /// <param name="exchange">The target exchange name.</param>
    /// <param name="routingKey">The routing key.</param>
    /// <param name="request">The request message.</param>
    /// <param name="options">Publishing options.</param>
    /// <param name="timeoutMs">Timeout in milliseconds.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response message.</returns>
    Task<TResponse> CallAsync<TRequest, TResponse>(
        string exchange,
        string routingKey,
        TRequest request,
        PublishOptions options,
        int timeoutMs = 30000,
        CancellationToken cancellationToken = default);
}
