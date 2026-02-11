using Flaminco.RabbitMQ.AMQP.Abstractions;
using Flaminco.RabbitMQ.AMQP.Attributes;
using Flaminco.RabbitMQ.AMQP.Configuration;
using Flaminco.RabbitMQ.AMQP.Observability;
using Flaminco.RabbitMQ.AMQP.Serialization;
using Flaminco.RabbitMQ.AMQP.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Reflection;

namespace Flaminco.RabbitMQ.AMQP.HostedServices;

/// <summary>
/// Hosted service that manages RabbitMQ consumer lifecycle.
/// </summary>
internal sealed class RabbitMqConsumerHostedService : IHostedService, IAsyncDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IRabbitMqConnectionManager _connectionManager;
    private readonly IMessageSerializer _serializer;
    private readonly TopologyInitializer _topologyInitializer;
    private readonly ConsumerTypeRegistry _consumerTypeRegistry;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqConsumerHostedService> _logger;

    private readonly ConcurrentDictionary<string, ConsumerRegistration> _consumers = new();
    private readonly ConcurrentDictionary<Type, ConsumerExecutionPlan> _consumerPlans = new();
    private readonly ConcurrentDictionary<Type, RpcExecutionPlan> _rpcPlans = new();
    private bool _disposed;

    public RabbitMqConsumerHostedService(IServiceProvider serviceProvider,
                                         IRabbitMqConnectionManager connectionManager,
                                         IMessageSerializer serializer,
                                         TopologyInitializer topologyInitializer,
                                         ConsumerTypeRegistry consumerTypeRegistry,
                                         IOptions<RabbitMqOptions> options,
                                         ILogger<RabbitMqConsumerHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _connectionManager = connectionManager;
        _serializer = serializer;
        _topologyInitializer = topologyInitializer;
        _consumerTypeRegistry = consumerTypeRegistry;
        _options = options.Value;
        _logger = logger;
    }

    private static bool IsRpcMessageConsumer(Type type)
    {
        Type? baseType = type.BaseType;
        while (baseType is not null)
        {
            if (baseType.IsGenericType &&
                baseType.GetGenericTypeDefinition() == typeof(RpcMessageConsumer<,>))
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting RabbitMQ consumer hosted service");

        try
        {
            // Initialize topology first
            await _topologyInitializer.InitializeAsync(cancellationToken);

            // Start all consumers
            foreach (var consumerType in _consumerTypeRegistry.ConsumerTypes)
            {
                await StartConsumerAsync(consumerType, cancellationToken);
            }

            _logger.LogInformation("Discovered {Count} message consumers", _consumerTypeRegistry.ConsumerTypes.Count);

            _logger.LogInformation("All RabbitMQ consumers started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start RabbitMQ consumers");
            throw;
        }
    }

    private async Task StartConsumerAsync(Type consumerType, CancellationToken cancellationToken)
    {
        var queueAttr = consumerType.GetCustomAttribute<QueueAttribute>()!;
        var queueName = queueAttr.Name;

        _logger.LogInformation("Starting consumer for queue '{Queue}' ({ConsumerType})",
            queueName, consumerType.Name);

        try
        {
            // Get or create a dedicated channel for this consumer
            IChannel channel = await _connectionManager.GetOrCreateChannelAsync(
                $"consumer:{queueName}",
                cancellationToken);

            // Set QoS
            ushort prefetchCount = queueAttr.PrefetchCount > 0 ? queueAttr.PrefetchCount : _options.DefaultPrefetchCount;

            await channel.BasicQosAsync(prefetchSize: 0,
                                        prefetchCount: prefetchCount,
                                        global: false,
                                        cancellationToken: cancellationToken);

            // Get the message type from the consumer
            Type messageType = GetMessageType(consumerType);

            // Create the event-based consumer
            var consumer = new AsyncEventingBasicConsumer(channel);
            var cts = new CancellationTokenSource();

            consumer.ReceivedAsync += async (sender, args) =>
            {
                await HandleMessageAsync(consumerType,
                                         messageType,
                                         queueAttr,
                                         channel,
                                         args,
                                         cts.Token);
            };

            // Start consuming
            string consumerTag = await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: queueAttr.AutoAck,
                consumer: consumer,
                cancellationToken: cancellationToken);

            // Store the registration
            _consumers[queueName] = new ConsumerRegistration(
                Channel: channel,
                Consumer: consumer,
                ConsumerTag: consumerTag,
                CancellationSource: cts,
                ConsumerType: consumerType,
                MessageType: messageType,
                QueueAttribute: queueAttr);

            _logger.LogInformation("Consumer started for queue '{Queue}' with tag '{Tag}'",
                queueName, consumerTag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start consumer for queue '{Queue}'", queueName);
            throw;
        }
    }

    private async Task HandleMessageAsync(
        Type consumerType,
        Type messageType,
        QueueAttribute queueAttr,
        IChannel channel,
        BasicDeliverEventArgs args,
        CancellationToken cancellationToken)
    {
        var queueName = queueAttr.Name;
        var startTimestamp = Stopwatch.GetTimestamp();
        using var activity = RabbitMqDiagnostics.ActivitySource.StartActivity("rabbitmq.consume", ActivityKind.Consumer);
        activity?.SetTag("messaging.system", "rabbitmq");
        activity?.SetTag("messaging.destination", queueName);
        activity?.SetTag("messaging.rabbitmq.routing_key", args.RoutingKey);
        activity?.SetTag("messaging.operation", "process");
        activity?.SetTag("messaging.message.id", args.BasicProperties.MessageId);
        activity?.SetTag("messaging.conversation_id", args.BasicProperties.CorrelationId);

        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var consumer = scope.ServiceProvider.GetRequiredService(consumerType);

            // Get retry count from headers
            var retryCount = GetRetryCount(args.BasicProperties);

            // Deserialize the message
            var message = _serializer.Deserialize(args.Body.Span, messageType);

            if (message is null)
            {
                _logger.LogWarning("Failed to deserialize message from queue '{Queue}'", queueName);
                // Only reject if manual acknowledgment is enabled
                if (!queueAttr.AutoAck)
                {
                    await channel.BasicRejectAsync(args.DeliveryTag, requeue: false, cancellationToken);
                }
                return;
            }

            // Check if this is an RPC consumer
            if (IsRpcMessageConsumer(consumerType))
            {
                await HandleRpcMessageAsync(consumerType, messageType, message, queueAttr, channel, args, consumer, cancellationToken);
            }
            else
            {
                await HandleRegularMessageAsync(consumerType, messageType, message, queueAttr, channel, args, retryCount, consumer, cancellationToken);
            }

            RabbitMqDiagnostics.ConsumerMessagesProcessed.Add(
                1,
                new KeyValuePair<string, object?>("queue", queueName),
                new KeyValuePair<string, object?>("consumer_type", consumerType.Name),
                new KeyValuePair<string, object?>("success", true));

            RabbitMqDiagnostics.ConsumerProcessingDurationMs.Record(
                Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds,
                new KeyValuePair<string, object?>("queue", queueName),
                new KeyValuePair<string, object?>("consumer_type", consumerType.Name));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message from queue '{Queue}'", queueName);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            RabbitMqDiagnostics.ConsumerMessagesFailed.Add(
                1,
                new KeyValuePair<string, object?>("queue", queueName),
                new KeyValuePair<string, object?>("consumer_type", consumerType.Name));
            await HandleMessageErrorAsync(consumerType, queueAttr, channel, args, ex, cancellationToken);

            RabbitMqDiagnostics.ConsumerProcessingDurationMs.Record(
                Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds,
                new KeyValuePair<string, object?>("queue", queueName),
                new KeyValuePair<string, object?>("consumer_type", consumerType.Name));
        }
    }

    private async Task HandleRpcMessageAsync(
        Type consumerType,
        Type messageType,
        object message,
        QueueAttribute queueAttr,
        IChannel channel,
        BasicDeliverEventArgs args,
        object consumer,
        CancellationToken cancellationToken)
    {
        var plan = _rpcPlans.GetOrAdd(consumerType, _ => CreateRpcExecutionPlan(consumerType, messageType));

        // Get response type from RpcMessageConsumer<TRequest, TResponse>
        var baseType = consumerType.BaseType!;
        _ = baseType.GetGenericArguments()[1];

        var messageId = args.BasicProperties.MessageId ?? Guid.NewGuid().ToString();
        var correlationId = args.BasicProperties.CorrelationId;
        var replyTo = args.BasicProperties.ReplyTo;
        var headers = args.BasicProperties.Headers;
        var timestamp = args.BasicProperties.Timestamp.UnixTime > 0
            ? DateTimeOffset.FromUnixTimeSeconds(args.BasicProperties.Timestamp.UnixTime).DateTime
            : DateTime.UtcNow;

        // Create respond function
        Func<object, Task> respondFunc = async (response) =>
        {
            if (string.IsNullOrEmpty(replyTo))
                throw new InvalidOperationException("Cannot respond: ReplyTo is not set");

            var responseBody = _serializer.Serialize(response);
            var replyProperties = new BasicProperties
            {
                CorrelationId = correlationId,
                ContentType = _serializer.ContentType,
                DeliveryMode = DeliveryModes.Transient
            };

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: replyTo,
                mandatory: false,
                basicProperties: replyProperties,
                body: responseBody,
                cancellationToken: cancellationToken);
        };

        // Create RpcContext
        var rpcContext = plan.ContextConstructor.Invoke(
            new object?[] { message, messageId, correlationId, replyTo, headers, timestamp, args.Exchange, args.RoutingKey, respondFunc });

        try
        {
            // Call HandleAsync method
            var responseTask = (Task)plan.HandleMethod.Invoke(consumer, new[] { rpcContext, cancellationToken })!;
            await responseTask;

            // Get response from task
            var response = plan.ResponseResultProperty.GetValue(responseTask)!;

            // Send response
            await respondFunc(response);

            // Acknowledge (only if manual acknowledgment is enabled)
            if (!queueAttr.AutoAck)
            {
                await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken);
            }

            _logger.LogDebug("Successfully processed RPC request from queue '{Queue}'", queueAttr.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling RPC request from queue '{Queue}'", queueAttr.Name);

            // Call OnErrorAsync
            var errorTask = (Task<ErrorHandlingResult>)plan.OnErrorMethod.Invoke(consumer, new[] { rpcContext, ex, cancellationToken })!;
            var result = await errorTask;

            // Only handle errors if manual acknowledgment is enabled
            if (!queueAttr.AutoAck)
            {
                switch (result)
                {
                    case ErrorHandlingResult.Acknowledge:
                        await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken);
                        break;
                    case ErrorHandlingResult.Reject:
                        await channel.BasicRejectAsync(args.DeliveryTag, requeue: false, cancellationToken);
                        break;
                    case ErrorHandlingResult.Requeue:
                        await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: true, cancellationToken);
                        break;
                }
            }

            throw;
        }
    }

    private async Task HandleRegularMessageAsync(
        Type consumerType,
        Type messageType,
        object message,
        QueueAttribute queueAttr,
        IChannel channel,
        BasicDeliverEventArgs args,
        int retryCount,
        object consumer,
        CancellationToken cancellationToken)
    {
        var plan = _consumerPlans.GetOrAdd(consumerType, _ => CreateConsumerExecutionPlan(consumerType, messageType));
        var context = Activator.CreateInstance(plan.ContextType)!;

        // Set context properties
        SetContextProperty(context, plan.MessageProperty, message);
        SetContextProperty(context, plan.BodyProperty, args.Body);
        SetContextProperty(context, plan.PropertiesProperty, args.BasicProperties);
        SetContextProperty(context, plan.DeliveryTagProperty, args.DeliveryTag);
        SetContextProperty(context, plan.ExchangeProperty, args.Exchange);
        SetContextProperty(context, plan.RoutingKeyProperty, args.RoutingKey);
        SetContextProperty(context, plan.RedeliveredProperty, args.Redelivered);
        SetContextProperty(context, plan.ConsumerTagProperty, args.ConsumerTag);
        SetContextProperty(context, plan.RetryCountProperty, retryCount);

        try
        {
            // Call OnBeforeConsumeAsync
            if (plan.BeforeMethod is not null)
            {
                var beforeResult = plan.BeforeMethod.Invoke(consumer, [context, cancellationToken]);
                if (beforeResult is Task<bool> beforeTask)
                {
                    var shouldContinue = await beforeTask;
                    if (!shouldContinue)
                    {
                        // Only acknowledge if manual acknowledgment is enabled
                        if (!queueAttr.AutoAck)
                        {
                            await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken);
                        }
                        return;
                    }
                }
            }

            // Call ConsumeAsync
            var consumeResult = plan.ConsumeMethod.Invoke(consumer, [context, cancellationToken]);
            if (consumeResult is Task consumeTask)
            {
                await consumeTask;
            }

            // Call OnAfterConsumeAsync
            if (plan.AfterMethod is not null)
            {
                var afterResult = plan.AfterMethod.Invoke(consumer, [context, cancellationToken]);
                if (afterResult is Task afterTask)
                {
                    await afterTask;
                }
            }

            // Acknowledge the message (only if manual acknowledgment is enabled)
            if (!queueAttr.AutoAck)
            {
                await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken);
            }

            _logger.LogDebug("Successfully processed message from queue '{Queue}'", queueAttr.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in regular consumer pipeline for queue '{Queue}'", queueAttr.Name);

            var errorTask = (Task<ErrorHandlingResult>)plan.OnErrorMethod.Invoke(consumer, new[] { context, ex, cancellationToken })!;
            var errorResult = await errorTask;

            await ApplyRegularErrorResultAsync(queueAttr, channel, args, errorResult, cancellationToken);
        }
    }

    private async Task HandleMessageErrorAsync(
        Type consumerType,
        QueueAttribute queueAttr,
        IChannel channel,
        BasicDeliverEventArgs args,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var retryCount = GetRetryCount(args.BasicProperties);

        // Only handle errors if manual acknowledgment is enabled
        // With auto-ack, messages are already removed from queue upon delivery
        if (queueAttr.AutoAck)
        {
            _logger.LogWarning(
                "Error processing message from queue '{Queue}' with AutoAck enabled. Message was already removed from queue.",
                queueAttr.Name);
            return;
        }

        // Check if we should retry
        if (queueAttr.RequeueOnFailure && retryCount < queueAttr.MaxRetryAttempts)
        {
            await RepublishForRetryAsync(queueAttr, channel, args, retryCount + 1, cancellationToken);
            await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken);
        }
        else
        {
            // Max retries reached, reject without requeue (will go to DLQ if configured)
            _logger.LogWarning(
                "Max retry attempts ({MaxRetries}) reached for message in queue '{Queue}'. Rejecting.",
                queueAttr.MaxRetryAttempts, queueAttr.Name);

            await channel.BasicRejectAsync(args.DeliveryTag, requeue: false, cancellationToken);
        }
    }

    private async Task ApplyRegularErrorResultAsync(
        QueueAttribute queueAttr,
        IChannel channel,
        BasicDeliverEventArgs args,
        ErrorHandlingResult result,
        CancellationToken cancellationToken)
    {
        if (queueAttr.AutoAck)
            return;

        switch (result)
        {
            case ErrorHandlingResult.Acknowledge:
                await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken);
                break;

            case ErrorHandlingResult.Requeue:
            {
                var retryCount = GetRetryCount(args.BasicProperties);
                if (retryCount < queueAttr.MaxRetryAttempts)
                {
                    await RepublishForRetryAsync(queueAttr, channel, args, retryCount + 1, cancellationToken);
                    await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken);
                }
                else
                {
                    _logger.LogWarning(
                        "Max retry attempts ({MaxRetries}) reached for message in queue '{Queue}'. Rejecting.",
                        queueAttr.MaxRetryAttempts,
                        queueAttr.Name);

                    await channel.BasicRejectAsync(args.DeliveryTag, requeue: false, cancellationToken);
                }
                break;
            }

            case ErrorHandlingResult.Reject:
            default:
                await channel.BasicRejectAsync(args.DeliveryTag, requeue: false, cancellationToken);
                break;
        }
    }

    private async Task RepublishForRetryAsync(
        QueueAttribute queueAttr,
        IChannel channel,
        BasicDeliverEventArgs args,
        int nextRetryCount,
        CancellationToken cancellationToken)
    {
        var headers = args.BasicProperties.Headers is not null
            ? new Dictionary<string, object?>(args.BasicProperties.Headers)
            : new Dictionary<string, object?>();

        headers["x-retry-count"] = nextRetryCount;

        var retryProperties = new BasicProperties
        {
            MessageId = args.BasicProperties.MessageId,
            CorrelationId = args.BasicProperties.CorrelationId,
            ReplyTo = args.BasicProperties.ReplyTo,
            ContentType = args.BasicProperties.ContentType,
            ContentEncoding = args.BasicProperties.ContentEncoding,
            DeliveryMode = args.BasicProperties.DeliveryMode,
            Priority = args.BasicProperties.Priority,
            Expiration = args.BasicProperties.Expiration,
            Type = args.BasicProperties.Type,
            UserId = args.BasicProperties.UserId,
            AppId = args.BasicProperties.AppId,
            Timestamp = args.BasicProperties.Timestamp,
            Headers = headers
        };

        await channel.BasicPublishAsync(
            exchange: args.Exchange,
            routingKey: args.RoutingKey,
            mandatory: false,
            basicProperties: retryProperties,
            body: args.Body,
            cancellationToken: cancellationToken);

        RabbitMqDiagnostics.ConsumerMessagesRetried.Add(
            1,
            new KeyValuePair<string, object?>("queue", queueAttr.Name),
            new KeyValuePair<string, object?>("retry_attempt", nextRetryCount));

        _logger.LogInformation(
            "Republished failed message from queue '{Queue}' for retry attempt {Retry}/{MaxRetries}",
            queueAttr.Name,
            nextRetryCount,
            queueAttr.MaxRetryAttempts);
    }

    private static int GetRetryCount(IReadOnlyBasicProperties properties)
    {
        if (properties.Headers?.TryGetValue("x-retry-count", out var value) == true)
        {
            return value switch
            {
                int intValue => intValue,
                long longValue => (int)longValue,
                byte[] bytes => BitConverter.ToInt32(bytes),
                _ => 0
            };
        }
        return 0;
    }

    private static Type GetMessageType(Type consumerType)
    {
        var baseType = consumerType.BaseType;
        while (baseType is not null)
        {
            if (baseType.IsGenericType &&
                baseType.GetGenericTypeDefinition() == typeof(MessageConsumer<>))
            {
                return baseType.GetGenericArguments()[0];
            }

            if (baseType.IsGenericType &&
                baseType.GetGenericTypeDefinition() == typeof(RpcMessageConsumer<,>))
            {
                return baseType.GetGenericArguments()[0]; // Request type
            }

            baseType = baseType.BaseType;
        }

        throw new InvalidOperationException(
            $"Consumer type {consumerType.Name} must extend MessageConsumer<T> or RpcMessageConsumer<TRequest, TResponse>");
    }

    private static void SetContextProperty(object context, PropertyInfo property, object value)
    {
        property.SetValue(context, value);
    }

    private static ConsumerExecutionPlan CreateConsumerExecutionPlan(Type consumerType, Type messageType)
    {
        var contextType = typeof(ConsumeContext<>).MakeGenericType(messageType);

        static PropertyInfo RequiredProperty(Type contextType, string name) =>
            contextType.GetProperty(name) ?? throw new InvalidOperationException($"Property '{name}' not found on '{contextType.Name}'");

        return new ConsumerExecutionPlan(
            ContextType: contextType,
            MessageProperty: RequiredProperty(contextType, "Message"),
            BodyProperty: RequiredProperty(contextType, "Body"),
            PropertiesProperty: RequiredProperty(contextType, "Properties"),
            DeliveryTagProperty: RequiredProperty(contextType, "DeliveryTag"),
            ExchangeProperty: RequiredProperty(contextType, "Exchange"),
            RoutingKeyProperty: RequiredProperty(contextType, "RoutingKey"),
            RedeliveredProperty: RequiredProperty(contextType, "Redelivered"),
            ConsumerTagProperty: RequiredProperty(contextType, "ConsumerTag"),
            RetryCountProperty: RequiredProperty(contextType, "RetryCount"),
            BeforeMethod: consumerType.GetMethod("OnBeforeConsumeAsync"),
            ConsumeMethod: consumerType.GetMethod("ConsumeAsync") ?? throw new InvalidOperationException($"ConsumeAsync not found for consumer '{consumerType.Name}'"),
            AfterMethod: consumerType.GetMethod("OnAfterConsumeAsync"),
            OnErrorMethod: consumerType.GetMethod("OnErrorAsync") ?? throw new InvalidOperationException($"OnErrorAsync not found for consumer '{consumerType.Name}'"));
    }

    private static RpcExecutionPlan CreateRpcExecutionPlan(Type consumerType, Type messageType)
    {
        var contextType = typeof(RpcContext<>).MakeGenericType(messageType);
        var constructor = contextType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .SingleOrDefault() ?? throw new InvalidOperationException($"Expected constructor for RPC context '{contextType.Name}'");

        var handleMethod = consumerType.GetMethod("HandleAsync")
            ?? throw new InvalidOperationException($"HandleAsync not found for RPC consumer '{consumerType.Name}'");

        var onErrorMethod = consumerType.GetMethod("OnErrorAsync")
            ?? throw new InvalidOperationException($"OnErrorAsync not found for RPC consumer '{consumerType.Name}'");

        var responseResultProperty = handleMethod.ReturnType.GetProperty("Result")
            ?? throw new InvalidOperationException($"Could not resolve response Result property for RPC consumer '{consumerType.Name}'");

        return new RpcExecutionPlan(
            ContextConstructor: constructor,
            HandleMethod: handleMethod,
            OnErrorMethod: onErrorMethod,
            ResponseResultProperty: responseResultProperty);
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping RabbitMQ consumers");

        foreach (var (queueName, registration) in _consumers)
        {
            try
            {
                await registration.CancellationSource.CancelAsync();

                if (registration.Channel.IsOpen)
                {
                    await registration.Channel.BasicCancelAsync(
                        registration.ConsumerTag,
                        cancellationToken: cancellationToken);
                }

                _logger.LogDebug("Stopped consumer for queue '{Queue}'", queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping consumer for queue '{Queue}'", queueName);
            }
        }

        _consumers.Clear();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var (_, registration) in _consumers)
        {
            try
            {
                await registration.Channel.CloseAsync();
                registration.Channel.Dispose();
            }
            catch { /* Ignore disposal errors */ }

            registration.CancellationSource.Dispose();
        }

        _consumers.Clear();
    }

    private sealed record ConsumerRegistration(
        IChannel Channel,
        AsyncEventingBasicConsumer Consumer,
        string ConsumerTag,
        CancellationTokenSource CancellationSource,
        Type ConsumerType,
        Type MessageType,
        QueueAttribute QueueAttribute);

    private sealed record ConsumerExecutionPlan(
        Type ContextType,
        PropertyInfo MessageProperty,
        PropertyInfo BodyProperty,
        PropertyInfo PropertiesProperty,
        PropertyInfo DeliveryTagProperty,
        PropertyInfo ExchangeProperty,
        PropertyInfo RoutingKeyProperty,
        PropertyInfo RedeliveredProperty,
        PropertyInfo ConsumerTagProperty,
        PropertyInfo RetryCountProperty,
        MethodInfo? BeforeMethod,
        MethodInfo ConsumeMethod,
        MethodInfo? AfterMethod,
        MethodInfo OnErrorMethod);

    private sealed record RpcExecutionPlan(
        ConstructorInfo ContextConstructor,
        MethodInfo HandleMethod,
        MethodInfo OnErrorMethod,
        PropertyInfo ResponseResultProperty);
}
