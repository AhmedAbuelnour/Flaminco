using Flaminco.RabbitMQ.AMQP.Abstractions;
using Flaminco.RabbitMQ.AMQP.Attributes;
using Flaminco.RabbitMQ.AMQP.Configuration;
using Flaminco.RabbitMQ.AMQP.Serialization;
using Flaminco.RabbitMQ.AMQP.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqConsumerHostedService> _logger;

    private readonly ConcurrentDictionary<string, ConsumerRegistration> _consumers = new();
    private readonly List<Type> _consumerTypes = [];
    private bool _disposed;

    public RabbitMqConsumerHostedService(
        IServiceProvider serviceProvider,
        IRabbitMqConnectionManager connectionManager,
        IMessageSerializer serializer,
        TopologyInitializer topologyInitializer,
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqConsumerHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _connectionManager = connectionManager;
        _serializer = serializer;
        _topologyInitializer = topologyInitializer;
        _options = options.Value;
        _logger = logger;

        // Discover all consumer types
        DiscoverConsumers();
    }

    private void DiscoverConsumers()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsAbstract &&
                        !type.IsInterface &&
                        IsMessageConsumer(type) &&
                        type.GetCustomAttribute<QueueAttribute>() is not null)
                    {
                        _consumerTypes.Add(type);
                    }
                }
            }
            catch (ReflectionTypeLoadException)
            {
                // Ignore assembly loading issues
            }
        }

        _logger.LogInformation("Discovered {Count} message consumers", _consumerTypes.Count);
    }

    private static bool IsMessageConsumer(Type type)
    {
        var baseType = type.BaseType;
        while (baseType is not null)
        {
            if (baseType.IsGenericType &&
                baseType.GetGenericTypeDefinition() == typeof(MessageConsumer<>))
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
            await _topologyInitializer.InitializeAsync(_consumerTypes, cancellationToken);

            // Start all consumers
            foreach (var consumerType in _consumerTypes)
            {
                await StartConsumerAsync(consumerType, cancellationToken);
            }

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
            var channel = await _connectionManager.GetOrCreateChannelAsync(
                $"consumer:{queueName}",
                cancellationToken);

            // Set QoS
            var prefetchCount = queueAttr.PrefetchCount > 0
                ? queueAttr.PrefetchCount
                : _options.DefaultPrefetchCount;

            await channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: prefetchCount,
                global: false,
                cancellationToken: cancellationToken);

            // Get the message type from the consumer
            var messageType = GetMessageType(consumerType);

            // Create the event-based consumer
            var consumer = new AsyncEventingBasicConsumer(channel);
            var cts = new CancellationTokenSource();

            consumer.ReceivedAsync += async (sender, args) =>
            {
                await HandleMessageAsync(
                    consumerType,
                    messageType,
                    queueAttr,
                    channel,
                    args,
                    cts.Token);
            };

            // Start consuming
            var consumerTag = await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
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
                await channel.BasicRejectAsync(args.DeliveryTag, requeue: false, cancellationToken);
                return;
            }

            // Create the context using reflection
            var contextType = typeof(ConsumeContext<>).MakeGenericType(messageType);
            var context = Activator.CreateInstance(contextType);

            // Set context properties
            SetContextProperty(context!, "Message", message);
            SetContextProperty(context!, "Body", args.Body);
            SetContextProperty(context!, "Properties", args.BasicProperties);
            SetContextProperty(context!, "DeliveryTag", args.DeliveryTag);
            SetContextProperty(context!, "Exchange", args.Exchange);
            SetContextProperty(context!, "RoutingKey", args.RoutingKey);
            SetContextProperty(context!, "Redelivered", args.Redelivered);
            SetContextProperty(context!, "ConsumerTag", args.ConsumerTag);
            SetContextProperty(context!, "RetryCount", retryCount);

            // Get lifecycle methods
            var beforeMethod = consumerType.GetMethod("OnBeforeConsumeAsync");
            var consumeMethod = consumerType.GetMethod("ConsumeAsync");
            var afterMethod = consumerType.GetMethod("OnAfterConsumeAsync");

            // Call OnBeforeConsumeAsync
            if (beforeMethod is not null)
            {
                var beforeResult = beforeMethod.Invoke(consumer, [context, cancellationToken]);
                if (beforeResult is Task<bool> beforeTask)
                {
                    var shouldContinue = await beforeTask;
                    if (!shouldContinue)
                    {
                        await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken);
                        return;
                    }
                }
            }

            // Call ConsumeAsync
            if (consumeMethod is not null)
            {
                var consumeResult = consumeMethod.Invoke(consumer, [context, cancellationToken]);
                if (consumeResult is Task consumeTask)
                {
                    await consumeTask;
                }
            }

            // Call OnAfterConsumeAsync
            if (afterMethod is not null)
            {
                var afterResult = afterMethod.Invoke(consumer, [context, cancellationToken]);
                if (afterResult is Task afterTask)
                {
                    await afterTask;
                }
            }

            // Acknowledge the message
            await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken);

            _logger.LogDebug("Successfully processed message from queue '{Queue}'", queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message from queue '{Queue}'", queueName);
            await HandleMessageErrorAsync(consumerType, queueAttr, channel, args, ex, cancellationToken);
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

        // Check if we should retry
        if (retryCount < queueAttr.MaxRetryAttempts)
        {
            // Requeue with incremented retry count
            var requeue = queueAttr.RequeueOnFailure;
            await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: requeue, cancellationToken);
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
            baseType = baseType.BaseType;
        }

        throw new InvalidOperationException(
            $"Consumer type {consumerType.Name} must extend MessageConsumer<T>");
    }

    private static void SetContextProperty(object context, string propertyName, object value)
    {
        var property = context.GetType().GetProperty(propertyName);
        property?.SetValue(context, value);
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
}
