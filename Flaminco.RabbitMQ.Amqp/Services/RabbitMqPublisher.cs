using Flaminco.RabbitMQ.AMQP.Abstractions;
using Flaminco.RabbitMQ.AMQP.Serialization;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Flaminco.RabbitMQ.AMQP.Services;

/// <summary>
/// Default implementation of <see cref="IMessagePublisher"/>.
/// </summary>
internal sealed class RabbitMqPublisher : IMessagePublisher
{
    private readonly IRabbitMqConnectionManager _connectionManager;
    private readonly IMessageSerializer _serializer;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(
        IRabbitMqConnectionManager connectionManager,
        IMessageSerializer serializer,
        ILogger<RabbitMqPublisher> logger)
    {
        _connectionManager = connectionManager;
        _serializer = serializer;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task PublishToQueueAsync<TMessage>(
        string queueName,
        TMessage message,
        PublishOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await PublishAsync(string.Empty, queueName, message, options, cancellationToken);
    }

    /// <inheritdoc />
    public async Task PublishAsync<TMessage>(
        string exchange,
        string routingKey,
        TMessage message,
        PublishOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new PublishOptions();

        using IChannel channel = await _connectionManager.CreateChannelAsync(cancellationToken);

        _logger.LogDebug("Publishing message to exchange '{Exchange}' with routing key '{RoutingKey}'",
            exchange, routingKey);

        await channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: routingKey,
            mandatory: options.Mandatory,
            basicProperties: CreateBasicProperties(options),
            body: _serializer.Serialize(message),
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> PublishWithConfirmAsync<TMessage>(
        string exchange,
        string routingKey,
        TMessage message,
        PublishOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new PublishOptions();

        using IChannel channel = await _connectionManager.CreateConfirmChannelAsync(cancellationToken);

        _logger.LogDebug("Publishing message with confirm to exchange '{Exchange}' with routing key '{RoutingKey}'",
            exchange, routingKey);

        try
        {
            // In RabbitMQ.Client 7.x, BasicPublishAsync with a confirm channel
            // automatically waits for broker confirmation
            await channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: options.Mandatory,
                basicProperties: CreateBasicProperties(options),
                body: _serializer.Serialize(message),
                cancellationToken: cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Publisher confirm failed for message to exchange '{Exchange}'", exchange);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task PublishBatchAsync<TMessage>(
        string exchange,
        string routingKey,
        IEnumerable<TMessage> messages,
        PublishOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new PublishOptions();

        using IChannel channel = await _connectionManager.CreateChannelAsync(cancellationToken);

        foreach (var message in messages)
        {
            await channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: options.Mandatory,
                basicProperties: CreateBasicProperties(options),
                body: _serializer.Serialize(message),
                cancellationToken: cancellationToken);
        }

        _logger.LogDebug("Published batch of messages to exchange '{Exchange}' with routing key '{RoutingKey}'",
            exchange, routingKey);
    }

    private BasicProperties CreateBasicProperties(PublishOptions options)
    {
        var properties = new BasicProperties
        {
            ContentType = _serializer.ContentType,
            DeliveryMode = options.Persistent ? DeliveryModes.Persistent : DeliveryModes.Transient,
            MessageId = options.MessageId ?? Guid.NewGuid().ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        if (!string.IsNullOrEmpty(options.CorrelationId))
            properties.CorrelationId = options.CorrelationId;

        if (!string.IsNullOrEmpty(options.ReplyTo))
            properties.ReplyTo = options.ReplyTo;

        if (options.Priority.HasValue)
            properties.Priority = options.Priority.Value;

        if (!string.IsNullOrEmpty(options.Expiration))
            properties.Expiration = options.Expiration;

        if (!string.IsNullOrEmpty(options.Type))
            properties.Type = options.Type;

        if (!string.IsNullOrEmpty(options.UserId))
            properties.UserId = options.UserId;

        if (!string.IsNullOrEmpty(options.AppId))
            properties.AppId = options.AppId;

        if (options.Headers.Count > 0)
            properties.Headers = options.Headers;

        return properties;
    }
}
