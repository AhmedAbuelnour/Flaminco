namespace Flaminco.RabbitMQ.AMQP.Abstractions;

/// <summary>
/// Provides message publishing capabilities to RabbitMQ.
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publishes a message to a queue using the default exchange.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="queueName">The queue name (used as routing key).</param>
    /// <param name="message">The message to publish.</param>
    /// <param name="options">Optional publish options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishToQueueAsync<TMessage>(
        string queueName,
        TMessage message,
        PublishOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a message to an exchange.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="exchange">The exchange name.</param>
    /// <param name="routingKey">The routing key.</param>
    /// <param name="message">The message to publish.</param>
    /// <param name="options">Optional publish options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishAsync<TMessage>(
        string exchange,
        string routingKey,
        TMessage message,
        PublishOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a message to an exchange with publisher confirms (reliable publishing).
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="exchange">The exchange name.</param>
    /// <param name="routingKey">The routing key.</param>
    /// <param name="message">The message to publish.</param>
    /// <param name="options">Optional publish options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the message was confirmed by the broker.</returns>
    Task<bool> PublishWithConfirmAsync<TMessage>(
        string exchange,
        string routingKey,
        TMessage message,
        PublishOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes multiple messages in a batch.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="exchange">The exchange name.</param>
    /// <param name="routingKey">The routing key.</param>
    /// <param name="messages">The messages to publish.</param>
    /// <param name="options">Optional publish options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishBatchAsync<TMessage>(
        string exchange,
        string routingKey,
        IEnumerable<TMessage> messages,
        PublishOptions? options = null,
        CancellationToken cancellationToken = default);
}
