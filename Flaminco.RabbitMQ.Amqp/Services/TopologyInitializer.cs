using Flaminco.RabbitMQ.AMQP.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Flaminco.RabbitMQ.AMQP.Services;

/// <summary>
/// Initializes RabbitMQ topology (exchanges, queues, bindings) from fluent API configuration.
/// </summary>
internal sealed class TopologyInitializer
{
    private readonly IRabbitMqConnectionManager _connectionManager;
    private readonly TopologyConfiguration _topology;
    private readonly ILogger<TopologyInitializer> _logger;

    public TopologyInitializer(
        IRabbitMqConnectionManager connectionManager,
        IOptions<TopologyConfiguration> topology,
        ILogger<TopologyInitializer> logger)
    {
        _connectionManager = connectionManager;
        _topology = topology.Value;
        _logger = logger;
    }

    /// <summary>
    /// Initializes topology from fluent API configuration only.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        using var channel = await _connectionManager.CreateChannelAsync(cancellationToken);

        // Initialize exchanges
        await InitializeExchangesAsync(channel, _topology.Exchanges, cancellationToken);

        // Initialize queues
        await InitializeQueuesAsync(channel, _topology.Queues, cancellationToken);

        // Initialize bindings
        await InitializeBindingsAsync(channel, _topology.Bindings, cancellationToken);

        _logger.LogInformation("RabbitMQ topology initialized successfully");
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
                arguments: exchange.Arguments.Count > 0 ? exchange.Arguments : null,
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
                arguments: binding.Arguments.Count > 0 ? binding.Arguments : null,
                cancellationToken: cancellationToken);
        }
    }
}
