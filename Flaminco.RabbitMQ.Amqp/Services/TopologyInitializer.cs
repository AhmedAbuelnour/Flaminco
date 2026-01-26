using Flaminco.RabbitMQ.AMQP.Attributes;
using Flaminco.RabbitMQ.AMQP.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Reflection;

namespace Flaminco.RabbitMQ.AMQP.Services;

/// <summary>
/// Initializes RabbitMQ topology (exchanges, queues, bindings) at application startup.
/// </summary>
internal sealed class TopologyInitializer
{
    private readonly IRabbitMqConnectionManager _connectionManager;
    private readonly TopologyConfiguration _topology;
    private readonly TopologyOptions _topologyOptions;
    private readonly ILogger<TopologyInitializer> _logger;

    public TopologyInitializer(
        IRabbitMqConnectionManager connectionManager,
        IOptions<TopologyConfiguration> topology,
        IOptions<TopologyOptions> topologyOptions,
        ILogger<TopologyInitializer> logger)
    {
        _connectionManager = connectionManager;
        _topology = topology.Value;
        _topologyOptions = topologyOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Initializes all configured topology from JSON configuration, fluent API, and attributes.
    /// </summary>
    /// <param name="consumerTypes">Consumer types to scan for attributes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task InitializeAsync(IEnumerable<Type> consumerTypes, CancellationToken cancellationToken = default)
    {
        using var channel = await _connectionManager.CreateChannelAsync(cancellationToken);

        // 1. Initialize topology from JSON configuration (hierarchical)
        await InitializeFromOptionsAsync(channel, cancellationToken);

        // 2. Initialize topology from fluent API configuration
        await InitializeExchangesAsync(channel, _topology.Exchanges, cancellationToken);
        await InitializeQueuesAsync(channel, _topology.Queues, cancellationToken);
        await InitializeBindingsAsync(channel, _topology.Bindings, cancellationToken);

        // 3. Initialize topology from attributes on consumers
        foreach (var consumerType in consumerTypes)
        {
            await InitializeFromAttributesAsync(channel, consumerType, cancellationToken);
        }

        _logger.LogInformation("RabbitMQ topology initialized successfully");
    }

    private async Task InitializeFromOptionsAsync(IChannel channel, CancellationToken cancellationToken)
    {
        // Process exchanges with their nested queues and bindings
        foreach (var exchange in _topologyOptions.Exchanges)
        {
            if (string.IsNullOrEmpty(exchange.Name)) continue;

            // Declare the exchange
            _logger.LogDebug("Declaring exchange '{Exchange}' ({Type})", exchange.Name, exchange.Type);

            await channel.ExchangeDeclareAsync(
                exchange: exchange.Name,
                type: exchange.Type,
                durable: exchange.Durable,
                autoDelete: exchange.AutoDelete,
                arguments: null,
                cancellationToken: cancellationToken);

            // Process queues bound to this exchange
            foreach (var queue in exchange.Queues)
            {
                if (string.IsNullOrEmpty(queue.Name)) continue;

                // Handle dead-letter setup first (create DLX and DLQ)
                if (queue.DeadLetter is not null)
                {
                    await SetupDeadLetterAsync(channel, exchange.Name, queue, cancellationToken);
                }

                // Declare the queue
                _logger.LogDebug("Declaring queue '{Queue}' bound to '{Exchange}'", queue.Name, exchange.Name);

                await channel.QueueDeclareAsync(
                    queue: queue.Name,
                    durable: queue.Durable,
                    exclusive: queue.Exclusive,
                    autoDelete: queue.AutoDelete,
                    arguments: queue.BuildArguments(),
                    cancellationToken: cancellationToken);

                // Bind queue to exchange
                _logger.LogDebug("Binding queue '{Queue}' to exchange '{Exchange}' with routing key '{RoutingKey}'",
                    queue.Name, exchange.Name, queue.RoutingKey);

                await channel.QueueBindAsync(
                    queue: queue.Name,
                    exchange: exchange.Name,
                    routingKey: queue.RoutingKey,
                    arguments: null,
                    cancellationToken: cancellationToken);
            }
        }

        // Process standalone queues (default exchange)
        foreach (var queue in _topologyOptions.Queues)
        {
            if (string.IsNullOrEmpty(queue.Name)) continue;

            // Handle dead-letter setup
            if (queue.DeadLetter is not null)
            {
                await SetupStandaloneDeadLetterAsync(channel, queue, cancellationToken);
            }

            _logger.LogDebug("Declaring standalone queue '{Queue}'", queue.Name);

            await channel.QueueDeclareAsync(
                queue: queue.Name,
                durable: queue.Durable,
                exclusive: queue.Exclusive,
                autoDelete: queue.AutoDelete,
                arguments: queue.BuildArguments(),
                cancellationToken: cancellationToken);
        }
    }

    private async Task SetupDeadLetterAsync(
        IChannel channel,
        string parentExchangeName,
        BoundQueueTopology queue,
        CancellationToken cancellationToken)
    {
        var dl = queue.DeadLetter!;

        // Determine DLX name (default: {parentExchange}.dlx)
        var dlxName = !string.IsNullOrEmpty(dl.Exchange)
            ? dl.Exchange
            : $"{parentExchangeName}.dlx";

        // Determine DLQ name (default: {queueName}.dlq)
        var dlqName = !string.IsNullOrEmpty(dl.Queue)
            ? dl.Queue
            : $"{queue.Name}.dlq";

        // Determine routing key (default: {queueName}.dlq)
        var dlRoutingKey = !string.IsNullOrEmpty(dl.RoutingKey)
            ? dl.RoutingKey
            : $"{queue.Name}.dlq";

        // Update the dead-letter config with resolved values
        dl.Exchange = dlxName;
        dl.RoutingKey = dlRoutingKey;

        // Declare dead-letter exchange
        _logger.LogDebug("Declaring dead-letter exchange '{DLX}'", dlxName);

        await channel.ExchangeDeclareAsync(
            exchange: dlxName,
            type: ExchangeTypes.Direct,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        // Declare dead-letter queue
        var dlqArgs = new Dictionary<string, object?>();
        if (dl.MessageTtl.HasValue)
            dlqArgs["x-message-ttl"] = dl.MessageTtl.Value;

        _logger.LogDebug("Declaring dead-letter queue '{DLQ}'", dlqName);

        await channel.QueueDeclareAsync(
            queue: dlqName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: dlqArgs.Count > 0 ? dlqArgs : null,
            cancellationToken: cancellationToken);

        // Bind DLQ to DLX
        _logger.LogDebug("Binding dead-letter queue '{DLQ}' to '{DLX}' with key '{Key}'",
            dlqName, dlxName, dlRoutingKey);

        await channel.QueueBindAsync(
            queue: dlqName,
            exchange: dlxName,
            routingKey: dlRoutingKey,
            arguments: null,
            cancellationToken: cancellationToken);
    }

    private async Task SetupStandaloneDeadLetterAsync(
        IChannel channel,
        QueueTopology queue,
        CancellationToken cancellationToken)
    {
        var dl = queue.DeadLetter!;

        // For standalone queues, use direct exchange or empty string
        var dlxName = !string.IsNullOrEmpty(dl.Exchange)
            ? dl.Exchange
            : ""; // Empty = default exchange

        var dlqName = !string.IsNullOrEmpty(dl.Queue)
            ? dl.Queue
            : $"{queue.Name}.dlq";

        var dlRoutingKey = !string.IsNullOrEmpty(dl.RoutingKey)
            ? dl.RoutingKey
            : dlqName;

        dl.Exchange = dlxName;
        dl.RoutingKey = dlRoutingKey;

        // If using a named DLX, declare it
        if (!string.IsNullOrEmpty(dlxName))
        {
            await channel.ExchangeDeclareAsync(
                exchange: dlxName,
                type: ExchangeTypes.Direct,
                durable: true,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);
        }

        // Declare DLQ
        var dlqArgs = new Dictionary<string, object?>();
        if (dl.MessageTtl.HasValue)
            dlqArgs["x-message-ttl"] = dl.MessageTtl.Value;

        await channel.QueueDeclareAsync(
            queue: dlqName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: dlqArgs.Count > 0 ? dlqArgs : null,
            cancellationToken: cancellationToken);

        // Bind if using named DLX
        if (!string.IsNullOrEmpty(dlxName))
        {
            await channel.QueueBindAsync(
                queue: dlqName,
                exchange: dlxName,
                routingKey: dlRoutingKey,
                arguments: null,
                cancellationToken: cancellationToken);
        }
    }

    private async Task InitializeExchangesAsync(
        IChannel channel,
        IEnumerable<ExchangeDeclaration> exchanges,
        CancellationToken cancellationToken)
    {
        foreach (var exchange in exchanges)
        {
            _logger.LogDebug("Declaring exchange '{Exchange}' of type '{Type}'", exchange.Name, exchange.Type);

            await channel.ExchangeDeclareAsync(
                exchange: exchange.Name,
                type: exchange.Type,
                durable: exchange.Durable,
                autoDelete: exchange.AutoDelete,
                arguments: exchange.Arguments,
                cancellationToken: cancellationToken);
        }
    }

    private async Task InitializeQueuesAsync(
        IChannel channel,
        IEnumerable<QueueDeclaration> queues,
        CancellationToken cancellationToken)
    {
        foreach (var queue in queues)
        {
            _logger.LogDebug("Declaring queue '{Queue}'", queue.Name);

            await channel.QueueDeclareAsync(
                queue: queue.Name,
                durable: queue.Durable,
                exclusive: queue.Exclusive,
                autoDelete: queue.AutoDelete,
                arguments: queue.BuildArguments(),
                cancellationToken: cancellationToken);
        }
    }

    private async Task InitializeBindingsAsync(
        IChannel channel,
        IEnumerable<BindingDeclaration> bindings,
        CancellationToken cancellationToken)
    {
        foreach (var binding in bindings)
        {
            _logger.LogDebug("Binding queue '{Queue}' to exchange '{Exchange}' with routing key '{RoutingKey}'",
                binding.Queue, binding.Exchange, binding.RoutingKey);

            await channel.QueueBindAsync(
                queue: binding.Queue,
                exchange: binding.Exchange,
                routingKey: binding.RoutingKey,
                arguments: binding.Arguments,
                cancellationToken: cancellationToken);
        }
    }

    private async Task InitializeFromAttributesAsync(
        IChannel channel,
        Type consumerType,
        CancellationToken cancellationToken)
    {
        // Declare exchanges from attributes
        foreach (var exchangeAttr in consumerType.GetCustomAttributes<ExchangeAttribute>())
        {
            _logger.LogDebug("Declaring exchange '{Exchange}' from attribute on '{Consumer}'",
                exchangeAttr.Name, consumerType.Name);

            await channel.ExchangeDeclareAsync(
                exchange: exchangeAttr.Name,
                type: exchangeAttr.Type,
                durable: exchangeAttr.Durable,
                autoDelete: exchangeAttr.AutoDelete,
                arguments: null,
                cancellationToken: cancellationToken);
        }

        // Declare queue from attribute
        var queueAttr = consumerType.GetCustomAttribute<QueueAttribute>();
        if (queueAttr is not null)
        {
            var arguments = new Dictionary<string, object?>();

            if (!string.IsNullOrEmpty(queueAttr.DeadLetterExchange))
                arguments["x-dead-letter-exchange"] = queueAttr.DeadLetterExchange;

            if (!string.IsNullOrEmpty(queueAttr.DeadLetterRoutingKey))
                arguments["x-dead-letter-routing-key"] = queueAttr.DeadLetterRoutingKey;

            if (queueAttr.MessageTtl > 0)
                arguments["x-message-ttl"] = queueAttr.MessageTtl;

            _logger.LogDebug("Declaring queue '{Queue}' from attribute on '{Consumer}'",
                queueAttr.Name, consumerType.Name);

            await channel.QueueDeclareAsync(
                queue: queueAttr.Name,
                durable: queueAttr.Durable,
                exclusive: queueAttr.Exclusive,
                autoDelete: queueAttr.AutoDelete,
                arguments: arguments.Count > 0 ? arguments : null,
                cancellationToken: cancellationToken);

            // Declare bindings from attributes
            foreach (var bindingAttr in consumerType.GetCustomAttributes<BindingAttribute>())
            {
                _logger.LogDebug("Binding queue '{Queue}' to exchange '{Exchange}' from attribute",
                    queueAttr.Name, bindingAttr.Exchange);

                await channel.QueueBindAsync(
                    queue: queueAttr.Name,
                    exchange: bindingAttr.Exchange,
                    routingKey: bindingAttr.RoutingKey,
                    arguments: null,
                    cancellationToken: cancellationToken);
            }
        }
    }
}
