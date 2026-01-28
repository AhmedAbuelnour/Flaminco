namespace Flaminco.RabbitMQ.AMQP.Abstractions;

/// <summary>
/// Context for RPC request handling with response capability.
/// </summary>
/// <typeparam name="TRequest">The request message type.</typeparam>
public sealed class RpcContext<TRequest>
{
    private readonly Func<object, Task> _respondFunc;

    /// <summary>
    /// Initializes a new instance of RpcContext.
    /// </summary>
    internal RpcContext(
        TRequest message,
        string messageId,
        string? correlationId,
        string? replyTo,
        IDictionary<string, object>? headers,
        DateTime timestamp,
        string exchange,
        string routingKey,
        Func<object, Task> respondFunc)
    {
        Message = message;
        MessageId = messageId;
        CorrelationId = correlationId;
        ReplyTo = replyTo;
        Headers = headers;
        Timestamp = timestamp;
        Exchange = exchange;
        RoutingKey = routingKey;
        _respondFunc = respondFunc;
    }

    /// <summary>
    /// Gets the deserialized request message.
    /// </summary>
    public TRequest Message { get; }

    /// <summary>
    /// Gets the message ID.
    /// </summary>
    public string MessageId { get; }

    /// <summary>
    /// Gets the correlation ID for tracking request-response pairs.
    /// </summary>
    public string? CorrelationId { get; }

    /// <summary>
    /// Gets the reply-to queue name.
    /// </summary>
    public string? ReplyTo { get; }

    /// <summary>
    /// Gets the message headers.
    /// </summary>
    public IDictionary<string, object>? Headers { get; }

    /// <summary>
    /// Gets the message timestamp.
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// Gets the exchange from which the message was received.
    /// </summary>
    public string Exchange { get; }

    /// <summary>
    /// Gets the routing key used to route the message.
    /// </summary>
    public string RoutingKey { get; }

    /// <summary>
    /// Gets whether this is an RPC request (has ReplyTo address).
    /// </summary>
    public bool IsRpcRequest => !string.IsNullOrEmpty(ReplyTo);

    /// <summary>
    /// Sends a response back to the caller.
    /// </summary>
    /// <typeparam name="TResponse">The response message type.</typeparam>
    /// <param name="response">The response message.</param>
    /// <returns>A task representing the async operation.</returns>
    public Task RespondAsync<TResponse>(TResponse response)
    {
        if (!IsRpcRequest)
            throw new InvalidOperationException("Cannot respond to a non-RPC message (ReplyTo is not set).");

        return _respondFunc(response!);
    }
}

