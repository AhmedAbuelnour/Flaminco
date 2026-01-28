namespace Flaminco.RabbitMQ.AMQP.Configuration;

/// <summary>
/// Fluent builder for RabbitMQ topology configuration.
/// </summary>
public sealed class TopologyConfiguration
{
    internal List<ExchangeDeclaration> Exchanges { get; } = [];
    internal List<QueueDeclaration> Queues { get; } = [];
    internal List<BindingDeclaration> Bindings { get; } = [];

    /// <summary>
    /// Configures an exchange with its queues and bindings.
    /// </summary>
    /// <param name="name">The exchange name.</param>
    /// <param name="configure">Action to configure the exchange.</param>
    /// <returns>The topology configuration for chaining.</returns>
    public TopologyConfiguration Exchange(string name, Action<ExchangeBuilder> configure)
    {
        var builder = new ExchangeBuilder(name, this);
        configure(builder);
        return this;
    }

    /// <summary>
    /// Configures a standalone queue (uses default exchange).
    /// </summary>
    /// <param name="name">The queue name.</param>
    /// <param name="configure">Optional action to configure the queue.</param>
    /// <returns>The topology configuration for chaining.</returns>
    public TopologyConfiguration Queue(string name, Action<StandaloneQueueBuilder>? configure = null)
    {
        var builder = new StandaloneQueueBuilder(name, this);
        configure?.Invoke(builder);
        builder.Build();
        return this;
    }
}

/// <summary>
/// Builder for configuring an exchange with its queues.
/// </summary>
public sealed class ExchangeBuilder
{
    private readonly string _name;
    private readonly TopologyConfiguration _topology;
    private string _type = ExchangeTypes.Direct;
    private bool _durable = true;
    private bool _autoDelete = false;
    private string? _dlxName;

    internal ExchangeBuilder(string name, TopologyConfiguration topology)
    {
        _name = name;
        _topology = topology;

        // Add the exchange declaration
        _topology.Exchanges.Add(new ExchangeDeclaration
        {
            Name = name,
            Type = _type,
            Durable = _durable,
            AutoDelete = _autoDelete
        });
    }

    /// <summary>
    /// Sets the exchange type.
    /// </summary>
    public ExchangeBuilder Type(string type)
    {
        _type = type;
        // Update the exchange declaration
        var exchange = _topology.Exchanges.First(e => e.Name == _name);
        exchange.Type = type;
        return this;
    }

    /// <summary>
    /// Sets the exchange as a topic exchange.
    /// </summary>
    public ExchangeBuilder Topic() => Type(ExchangeTypes.Topic);

    /// <summary>
    /// Sets the exchange as a fanout exchange.
    /// </summary>
    public ExchangeBuilder Fanout() => Type(ExchangeTypes.Fanout);

    /// <summary>
    /// Sets the exchange as a direct exchange.
    /// </summary>
    public ExchangeBuilder Direct() => Type(ExchangeTypes.Direct);

    /// <summary>
    /// Sets the exchange as a headers exchange.
    /// </summary>
    public ExchangeBuilder Headers() => Type(ExchangeTypes.Headers);

    /// <summary>
    /// Sets whether the exchange is durable.
    /// </summary>
    public ExchangeBuilder Durable(bool durable = true)
    {
        _durable = durable;
        var exchange = _topology.Exchanges.First(e => e.Name == _name);
        exchange.Durable = durable;
        return this;
    }

    /// <summary>
    /// Configures the dead-letter exchange for all queues in this exchange.
    /// </summary>
    /// <param name="dlxName">Custom DLX name. Defaults to {exchangeName}.dlx</param>
    public ExchangeBuilder WithDeadLetterExchange(string? dlxName = null)
    {
        _dlxName = dlxName ?? $"{_name}.dlx";

        // Add the DLX declaration
        _topology.Exchanges.Add(new ExchangeDeclaration
        {
            Name = _dlxName,
            Type = ExchangeTypes.Direct,
            Durable = true,
            AutoDelete = false
        });

        return this;
    }

    /// <summary>
    /// Adds a queue bound to this exchange.
    /// </summary>
    /// <param name="name">The queue name.</param>
    /// <param name="bindingKey">The binding key for queue.</param>
    /// <param name="configure">Optional action to configure the queue.</param>
    public ExchangeBuilder Queue(string name, string bindingKey = "", Action<BoundQueueBuilder>? configure = null)
    {
        var builder = new BoundQueueBuilder(name, _name, _dlxName, _topology);
        builder.RoutingKey(bindingKey);
        configure?.Invoke(builder);
        builder.Build();
        return this;
    }
}

/// <summary>
/// Builder for configuring a queue bound to an exchange.
/// </summary>
public sealed class BoundQueueBuilder
{
    private readonly string _name;
    private readonly string _exchangeName;
    private readonly string? _defaultDlx;
    private readonly TopologyConfiguration _topology;

    private string _routingKey = "";
    private bool _durable = true;
    private bool _exclusive = false;
    private bool _autoDelete = false;
    private int? _messageTtl;
    private int? _maxLength;
    private long? _maxLengthBytes;
    private int? _maxPriority;
    private string? _queueMode;

    private bool _hasDeadLetter = false;
    private string? _dlxName;
    private string? _dlqName;
    private string? _dlRoutingKey;
    private int? _dlqMessageTtl;

    internal BoundQueueBuilder(string name, string exchangeName, string? defaultDlx, TopologyConfiguration topology)
    {
        _name = name;
        _exchangeName = exchangeName;
        _defaultDlx = defaultDlx;
        _topology = topology;
    }

    /// <summary>
    /// Sets the routing key for binding to the exchange.
    /// </summary>
    public BoundQueueBuilder RoutingKey(string routingKey)
    {
        _routingKey = routingKey;
        return this;
    }

    /// <summary>
    /// Sets whether the queue is durable.
    /// </summary>
    public BoundQueueBuilder Durable(bool durable = true)
    {
        _durable = durable;
        return this;
    }

    /// <summary>
    /// Sets the message TTL in milliseconds.
    /// </summary>
    public BoundQueueBuilder MessageTtl(int ttlMs)
    {
        _messageTtl = ttlMs;
        return this;
    }

    /// <summary>
    /// Sets the maximum queue length.
    /// </summary>
    public BoundQueueBuilder MaxLength(int maxLength)
    {
        _maxLength = maxLength;
        return this;
    }

    /// <summary>
    /// Sets the maximum queue size in bytes.
    /// </summary>
    public BoundQueueBuilder MaxLengthBytes(long maxBytes)
    {
        _maxLengthBytes = maxBytes;
        return this;
    }

    /// <summary>
    /// Enables priority queue with the specified max priority.
    /// </summary>
    public BoundQueueBuilder MaxPriority(int maxPriority)
    {
        _maxPriority = maxPriority;
        return this;
    }

    /// <summary>
    /// Sets the queue mode (e.g., "lazy").
    /// </summary>
    public BoundQueueBuilder QueueMode(string mode)
    {
        _queueMode = mode;
        return this;
    }

    /// <summary>
    /// Configures dead-letter for this queue.
    /// </summary>
    /// <param name="configure">Optional action to customize DLQ settings.</param>
    public BoundQueueBuilder DeadLetter(Action<DeadLetterBuilder>? configure = null)
    {
        _hasDeadLetter = true;
        _dlxName = _defaultDlx ?? $"{_exchangeName}.dlx";
        _dlqName = $"{_name}.dlq";
        _dlRoutingKey = $"{_name}.dlq";

        if (configure is not null)
        {
            var builder = new DeadLetterBuilder();
            configure(builder);
            if (builder.ExchangeName is not null) _dlxName = builder.ExchangeName;
            if (builder.QueueName is not null) _dlqName = builder.QueueName;
            if (builder.RoutingKeyValue is not null) _dlRoutingKey = builder.RoutingKeyValue;
            if (builder.MessageTtlValue is not null) _dlqMessageTtl = builder.MessageTtlValue;
        }

        return this;
    }

    internal void Build()
    {
        // Create DLX and DLQ if configured
        if (_hasDeadLetter)
        {
            // Add DLX if not already added
            if (!_topology.Exchanges.Any(e => e.Name == _dlxName))
            {
                _topology.Exchanges.Add(new ExchangeDeclaration
                {
                    Name = _dlxName!,
                    Type = ExchangeTypes.Direct,
                    Durable = true,
                    AutoDelete = false
                });
            }

            // Add DLQ
            _topology.Queues.Add(new QueueDeclaration
            {
                Name = _dlqName!,
                Durable = true,
                MessageTtl = _dlqMessageTtl
            });

            // Bind DLQ to DLX
            _topology.Bindings.Add(new BindingDeclaration
            {
                Queue = _dlqName!,
                Exchange = _dlxName!,
                RoutingKey = _dlRoutingKey!
            });
        }

        // Create the main queue
        _topology.Queues.Add(new QueueDeclaration
        {
            Name = _name,
            Durable = _durable,
            Exclusive = _exclusive,
            AutoDelete = _autoDelete,
            DeadLetterExchange = _hasDeadLetter ? _dlxName : null,
            DeadLetterRoutingKey = _hasDeadLetter ? _dlRoutingKey : null,
            MessageTtl = _messageTtl,
            MaxLength = _maxLength,
            MaxLengthBytes = _maxLengthBytes
        });

        // Bind queue to exchange
        _topology.Bindings.Add(new BindingDeclaration
        {
            Queue = _name,
            Exchange = _exchangeName,
            RoutingKey = _routingKey
        });
    }
}

/// <summary>
/// Builder for configuring a standalone queue.
/// </summary>
public sealed class StandaloneQueueBuilder
{
    private readonly string _name;
    private readonly TopologyConfiguration _topology;

    private bool _durable = true;
    private bool _exclusive = false;
    private bool _autoDelete = false;
    private int? _messageTtl;
    private int? _maxLength;

    private bool _hasDeadLetter = false;
    private string? _dlqName;

    internal StandaloneQueueBuilder(string name, TopologyConfiguration topology)
    {
        _name = name;
        _topology = topology;
    }

    /// <summary>
    /// Sets whether the queue is durable.
    /// </summary>
    public StandaloneQueueBuilder Durable(bool durable = true)
    {
        _durable = durable;
        return this;
    }

    /// <summary>
    /// Sets the message TTL in milliseconds.
    /// </summary>
    public StandaloneQueueBuilder MessageTtl(int ttlMs)
    {
        _messageTtl = ttlMs;
        return this;
    }

    /// <summary>
    /// Sets the maximum queue length.
    /// </summary>
    public StandaloneQueueBuilder MaxLength(int maxLength)
    {
        _maxLength = maxLength;
        return this;
    }

    /// <summary>
    /// Configures dead-letter for this queue (uses default exchange for DLQ).
    /// </summary>
    /// <param name="dlqName">Custom DLQ name. Defaults to {queueName}.dlq</param>
    public StandaloneQueueBuilder DeadLetter(string? dlqName = null)
    {
        _hasDeadLetter = true;
        _dlqName = dlqName ?? $"{_name}.dlq";
        return this;
    }

    internal void Build()
    {
        // Create DLQ if configured
        if (_hasDeadLetter)
        {
            _topology.Queues.Add(new QueueDeclaration
            {
                Name = _dlqName!,
                Durable = true
            });
        }

        // Create the main queue
        _topology.Queues.Add(new QueueDeclaration
        {
            Name = _name,
            Durable = _durable,
            Exclusive = _exclusive,
            AutoDelete = _autoDelete,
            DeadLetterExchange = _hasDeadLetter ? "" : null, // Empty string = default exchange
            DeadLetterRoutingKey = _hasDeadLetter ? _dlqName : null,
            MessageTtl = _messageTtl,
            MaxLength = _maxLength
        });
    }
}

/// <summary>
/// Builder for configuring dead-letter settings.
/// </summary>
public sealed class DeadLetterBuilder
{
    internal string? ExchangeName { get; private set; }
    internal string? QueueName { get; private set; }
    internal string? RoutingKeyValue { get; private set; }
    internal int? MessageTtlValue { get; private set; }

    /// <summary>
    /// Sets a custom dead-letter exchange name.
    /// </summary>
    public DeadLetterBuilder Exchange(string name)
    {
        ExchangeName = name;
        return this;
    }

    /// <summary>
    /// Sets a custom dead-letter queue name.
    /// </summary>
    public DeadLetterBuilder Queue(string name)
    {
        QueueName = name;
        return this;
    }

    /// <summary>
    /// Sets a custom routing key for the DLQ binding.
    /// </summary>
    public DeadLetterBuilder RoutingKey(string routingKey)
    {
        RoutingKeyValue = routingKey;
        return this;
    }

    /// <summary>
    /// Sets the message TTL for the DLQ (useful for retry delays).
    /// </summary>
    public DeadLetterBuilder MessageTtl(int ttlMs)
    {
        MessageTtlValue = ttlMs;
        return this;
    }
}
