using Flaminco.RabbitMQ.AMQP.Abstractions;
using Flaminco.RabbitMQ.AMQP.Serialization;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;

namespace Flaminco.RabbitMQ.AMQP.Services;

/// <summary>
/// RPC client implementation for making request-reply calls over RabbitMQ.
/// </summary>
internal sealed class RabbitMqRpcClient : IMessageRpcClient, IAsyncDisposable
{
    private readonly IRabbitMqConnectionManager _connectionManager;
    private readonly IMessageSerializer _serializer;
    private readonly ILogger<RabbitMqRpcClient> _logger;

    private readonly ConcurrentDictionary<string, TaskCompletionSource<(byte[] Body, string ContentType)>> _pendingCalls = new();
    private IChannel? _channel;
    private string? _replyQueueName;
    private bool _initialized;
    private bool _disposed;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public RabbitMqRpcClient(
        IRabbitMqConnectionManager connectionManager,
        IMessageSerializer serializer,
        ILogger<RabbitMqRpcClient> logger)
    {
        _connectionManager = connectionManager;
        _serializer = serializer;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TResponse> CallAsync<TRequest, TResponse>(
        string exchange,
        string routingKey,
        TRequest request,
        int timeoutMs = 30000,
        CancellationToken cancellationToken = default)
    {
        var options = new PublishOptions();
        return await CallAsync<TRequest, TResponse>(exchange, routingKey, request, options, timeoutMs, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TResponse> CallAsync<TRequest, TResponse>(
        string exchange,
        string routingKey,
        TRequest request,
        PublishOptions options,
        int timeoutMs = 30000,
        CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        var correlationId = options.CorrelationId ?? Guid.NewGuid().ToString();
        var messageId = options.MessageId ?? Guid.NewGuid().ToString();

        // Create a task completion source for this call
        var tcs = new TaskCompletionSource<(byte[] Body, string ContentType)>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingCalls[correlationId] = tcs;

        try
        {
            // Serialize the request
            var body = _serializer.Serialize(request);

            // Prepare message properties
            var properties = new BasicProperties
            {
                MessageId = messageId,
                CorrelationId = correlationId,
                ReplyTo = _replyQueueName,
                ContentType = _serializer.ContentType,
                DeliveryMode = options.Persistent ? DeliveryModes.Persistent : DeliveryModes.Transient,
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                Priority = options.Priority ?? 0,
                Expiration = options.Expiration,
                Type = options.Type,
                UserId = options.UserId,
                AppId = options.AppId,
                Headers = options.Headers.Count > 0 ? new Dictionary<string, object?>(options.Headers) : null
            };

            _logger.LogDebug("Making RPC call to {Exchange}/{RoutingKey} with correlation ID {CorrelationId}",
                exchange, routingKey, correlationId);

            // Publish the request
            await _channel!.BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            // Wait for response with timeout
            using var timeoutCts = new CancellationTokenSource(timeoutMs);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var registration = linkedCts.Token.Register(() =>
            {
                tcs.TrySetCanceled();
            });

            try
            {
                var (responseBody, contentType) = await tcs.Task;

                // Deserialize response
                var response = _serializer.Deserialize<TResponse>(responseBody);

                _logger.LogDebug("RPC call completed successfully for correlation ID {CorrelationId}", correlationId);

                return response!;
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                throw new TimeoutException($"RPC call timed out after {timeoutMs}ms (CorrelationId: {correlationId})");
            }
            finally
            {
                await registration.DisposeAsync();
            }
        }
        finally
        {
            _pendingCalls.TryRemove(correlationId, out _);
        }
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_initialized) return;

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized) return;

            _logger.LogInformation("Initializing RPC client with callback queue");

            // Create a dedicated channel for RPC
            _channel = await _connectionManager.GetOrCreateChannelAsync("rpc-client", cancellationToken);

            // Declare an exclusive callback queue
            var queueDeclareResult = await _channel.QueueDeclareAsync(
                queue: "",
                durable: false,
                exclusive: true,
                autoDelete: true,
                arguments: null,
                cancellationToken: cancellationToken);

            _replyQueueName = queueDeclareResult.QueueName;

            _logger.LogInformation("RPC callback queue created: {QueueName}", _replyQueueName);

            // Start consuming responses
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += OnResponseReceivedAsync;

            await _channel.BasicConsumeAsync(
                queue: _replyQueueName,
                autoAck: true,
                consumer: consumer,
                cancellationToken: cancellationToken);

            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private Task OnResponseReceivedAsync(object sender, BasicDeliverEventArgs args)
    {
        var correlationId = args.BasicProperties.CorrelationId;

        if (string.IsNullOrEmpty(correlationId))
        {
            _logger.LogWarning("Received RPC response without correlation ID");
            return Task.CompletedTask;
        }

        if (_pendingCalls.TryGetValue(correlationId, out var tcs))
        {
            var contentType = args.BasicProperties.ContentType ?? _serializer.ContentType;
            tcs.TrySetResult((args.Body.ToArray(), contentType));
        }
        else
        {
            _logger.LogWarning("Received RPC response for unknown correlation ID: {CorrelationId}", correlationId);
        }

        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        _disposed = true;

        // Cancel all pending calls
        foreach (var tcs in _pendingCalls.Values)
        {
            tcs.TrySetCanceled();
        }

        _pendingCalls.Clear();

        if (_channel is not null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }

        _initLock.Dispose();

        _logger.LogInformation("RPC client disposed");
    }
}
