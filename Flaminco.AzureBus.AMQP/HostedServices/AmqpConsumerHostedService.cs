namespace Flaminco.AzureBus.AMQP.HostedServices
{
    using Azure.Messaging.ServiceBus;
    using Flaminco.AzureBus.AMQP.Abstractions;
    using Flaminco.AzureBus.AMQP.Attributes;
    using Flaminco.AzureBus.AMQP.Services;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System.Reflection;

    /// <summary>
    /// A hosted service responsible for automatically discovering, initializing, and managing the lifecycle of
    /// Azure Service Bus message consumers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This service acts as the orchestrator for all Azure Service Bus message consumers in the application:
    /// <list type="bullet">
    ///   <item><description>Discovers consumer classes decorated with <see cref="QueueConsumerAttribute"/> and <see cref="TopicConsumerAttribute"/></description></item>
    ///   <item><description>Creates and configures the appropriate <see cref="ServiceBusProcessor"/> instances</description></item>
    ///   <item><description>Manages the lifecycle of all processors (start, stop, dispose)</description></item>
    ///   <item><description>Routes incoming messages to the appropriate consumer instances</description></item>
    ///   <item><description>Handles message completion, abandonment, and error scenarios</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The service is automatically registered by the <see cref="Extensions.AmqpClientExtensions.AddAmqpClient"/> method
    /// and starts when the application starts.
    /// </para>
    /// </remarks>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AmqpConsumerHostedService"/> class.
    /// </remarks>
    /// <param name="serviceProvider">The service provider used to resolve consumer instances.</param>
    /// <param name="connectionProvider">The Azure Service Bus connection provider.</param>
    /// <param name="logger">The logger for diagnostic information.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the required dependencies are null.</exception>
    internal sealed class AmqpConsumerHostedService(IServiceProvider serviceProvider,
                                                    AmqpConnectionProvider connectionProvider,
                                                    ILogger<AmqpConsumerHostedService> logger) : IHostedService, IAsyncDisposable
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        private readonly AmqpConnectionProvider _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        private readonly ILogger<AmqpConsumerHostedService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly List<(ServiceBusProcessor Processor, CancellationTokenSource CancellationSource, Type ConsumerType)> _queueProcessors = new();
        private readonly List<(ServiceBusProcessor Processor, CancellationTokenSource CancellationSource, Type ConsumerType)> _topicProcessors = new();
        private bool _disposed;

        /// <summary>
        /// Starts the Azure Service Bus consumer processors.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method is called when the application starts and performs the following operations:
        /// <list type="number">
        ///   <item><description>Discovers and sets up all queue consumers in the application</description></item>
        ///   <item><description>Discovers and sets up all topic consumers in the application</description></item>
        ///   <item><description>Starts all processors to begin receiving messages</description></item>
        /// </list>
        /// </remarks>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Azure Service Bus consumers");

            // Find and set up queue consumers
            await SetupQueueConsumers(cancellationToken);

            // Find and set up topic consumers
            await SetupTopicConsumers(cancellationToken);
        }

        /// <summary>
        /// Discovers and sets up all queue consumers in the application.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task SetupQueueConsumers(CancellationToken cancellationToken)
        {
            // Find all types with QueueConsumerAttribute
            var queueConsumerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetCustomAttributes<QueueConsumerAttribute>().Any())
                .ToList();

            foreach (var consumerType in queueConsumerTypes)
            {
                var queueAttribute = consumerType.GetCustomAttribute<QueueConsumerAttribute>();
                if (queueAttribute == null)
                    continue;

                try
                {
                    _logger.LogInformation("Starting consumer for queue {Queue}", queueAttribute.Queue);

                    // Get message type from the MessageConsumer<T> base class
                    Type? messageConsumerBaseType = consumerType.BaseType;
                    while (messageConsumerBaseType != null && (!messageConsumerBaseType.IsGenericType ||
                           messageConsumerBaseType.GetGenericTypeDefinition() != typeof(MessageConsumer<>)))
                    {
                        messageConsumerBaseType = messageConsumerBaseType.BaseType;
                    }

                    if (messageConsumerBaseType == null)
                    {
                        _logger.LogError("Could not find MessageConsumer<T> base type for consumer {Consumer}", consumerType.Name);
                        continue;
                    }

                    // Create a processor for this queue - this will automatically create the queue if it doesn't exist
                    ServiceBusProcessor processor = await _connectionProvider.CreateQueueProcessorAsync(queueAttribute.Queue, cancellationToken);

                    // Create a cancellation token source for this processor
                    CancellationTokenSource processorCts = new();

                    // Set up message and error handlers
                    processor.ProcessMessageAsync += async (args) => await ProcessMessageAsync(consumerType, args, processorCts.Token);
                    processor.ProcessErrorAsync += ProcessErrorAsync;

                    // Start the processor
                    await processor.StartProcessingAsync(cancellationToken);

                    // Store processor information
                    _queueProcessors.Add((processor, processorCts, consumerType));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error setting up consumer for queue {Queue}", queueAttribute.Queue);
                }
            }
        }

        /// <summary>
        /// Discovers and sets up all topic consumers in the application.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task SetupTopicConsumers(CancellationToken cancellationToken)
        {
            // Find all types with TopicConsumerAttribute
            var topicConsumerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetCustomAttributes<TopicConsumerAttribute>().Any())
                .ToList();

            foreach (Type consumerType in topicConsumerTypes)
            {
                TopicConsumerAttribute? topicAttribute = consumerType.GetCustomAttribute<TopicConsumerAttribute>();

                if (topicAttribute == null)
                    continue;

                try
                {
                    _logger.LogInformation("Starting consumer for topic {Topic}, subscription {Subscription}",
                        topicAttribute.Topic, topicAttribute.Subscription);

                    // Get message type from the MessageConsumer<T> base class
                    Type? messageConsumerBaseType = consumerType.BaseType;
                    while (messageConsumerBaseType != null && (!messageConsumerBaseType.IsGenericType ||
                           messageConsumerBaseType.GetGenericTypeDefinition() != typeof(MessageConsumer<>)))
                    {
                        messageConsumerBaseType = messageConsumerBaseType.BaseType;
                    }

                    if (messageConsumerBaseType == null)
                    {
                        _logger.LogError("Could not find MessageConsumer<T> base type for consumer {Consumer}", consumerType.Name);
                        continue;
                    }

                    // Create a processor for this topic/subscription - this will automatically create the topic and subscription if they don't exist
                    ServiceBusProcessor processor = await _connectionProvider.CreateTopicProcessorAsync(topicAttribute.Topic, topicAttribute.Subscription, cancellationToken);

                    // Create a cancellation token source for this processor
                    CancellationTokenSource processorCts = new();

                    // Set up message and error handlers
                    processor.ProcessMessageAsync += async (args) => await ProcessMessageAsync(consumerType, args, processorCts.Token);
                    processor.ProcessErrorAsync += ProcessErrorAsync;

                    // Start the processor
                    await processor.StartProcessingAsync(cancellationToken);

                    // Store processor information
                    _topicProcessors.Add((processor, processorCts, consumerType));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error setting up consumer for topic {Topic}, subscription {Subscription}", topicAttribute.Topic, topicAttribute.Subscription);
                }
            }
        }

        /// <summary>
        /// Processes a message received by a Service Bus processor.
        /// </summary>
        /// <param name="consumerType">The type of the consumer handling the message.</param>
        /// <param name="args">The message processing arguments.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ProcessMessageAsync(Type consumerType, ProcessMessageEventArgs args, CancellationToken cancellationToken)
        {
            try
            {
                // Create a scoped service provider to resolve the consumer instance
                AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();

                // Resolve the consumer instance
                object consumerInstance = scope.ServiceProvider.GetRequiredService(consumerType);

                // Find and invoke the ProcessMessageAsync method via reflection
                MethodInfo? processMethod = consumerType.BaseType?.GetMethod("ProcessMessageAsync", BindingFlags.NonPublic | BindingFlags.Instance);

                if (processMethod != null)
                {
                    // Invoke the method
                    if (processMethod.Invoke(consumerInstance, [args.Message, cancellationToken]) is Task processTask)
                    {
                        await processTask;

                        // Complete the message
                        await args.CompleteMessageAsync(args.Message, cancellationToken);
                    }
                }
                else
                {
                    _logger.LogError("ProcessMessageAsync method not found on consumer {Consumer}", consumerType.Name);
                    // Abandon the message so it can be reprocessed
                    await args.AbandonMessageAsync(args.Message, null, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message for consumer {Consumer}", consumerType.Name);
                // Abandon the message so it can be reprocessed
                await args.AbandonMessageAsync(args.Message, null, cancellationToken);
            }
        }

        /// <summary>
        /// Handles errors that occur during message processing.
        /// </summary>
        /// <param name="args">The error processing arguments.</param>
        /// <returns>A completed task.</returns>
        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Error processing message: {ErrorSource}, {EntityPath}", args.ErrorSource, args.EntityPath);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops the Azure Service Bus consumer processors.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method is called when the application stops and performs the following operations:
        /// <list type="number">
        ///   <item><description>Stops all queue processors</description></item>
        ///   <item><description>Stops all topic processors</description></item>
        ///   <item><description>Clears the internal processor lists</description></item>
        /// </list>
        /// </remarks>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Azure Service Bus consumers");

            // Stop queue processors
            foreach ((ServiceBusProcessor processor, CancellationTokenSource cts, Type _) in _queueProcessors)
            {
                try
                {
                    // Cancel any ongoing processing
                    await cts.CancelAsync();

                    // Stop the processor
                    await processor.StopProcessingAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error stopping queue processor");
                }
            }

            // Stop topic processors
            foreach ((ServiceBusProcessor processor, CancellationTokenSource cts, Type _) in _topicProcessors)
            {
                try
                {
                    // Cancel any ongoing processing
                    await cts.CancelAsync();

                    // Stop the processor
                    await processor.StopProcessingAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error stopping topic processor");
                }
            }

            _queueProcessors.Clear();
            _topicProcessors.Clear();
        }

        /// <summary>
        /// Disposes the Azure Service Bus consumer processors and associated resources.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            // Dispose all queue processors
            foreach (var (processor, cts, _) in _queueProcessors)
            {
                try
                {
                    await processor.StopProcessingAsync();
                    await processor.DisposeAsync();
                }
                catch { }

                cts.Dispose();
            }

            // Dispose all topic processors
            foreach ((ServiceBusProcessor processor, CancellationTokenSource cts, Type _) in _topicProcessors)
            {
                try
                {
                    await processor.StopProcessingAsync();
                    await processor.DisposeAsync();
                }
                catch { }

                cts.Dispose();
            }

            _queueProcessors.Clear();
            _topicProcessors.Clear();
            _disposed = true;

            GC.SuppressFinalize(this);
        }
    }
}
