using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Flaminco.AzureBus.AMQP.Services
{
    /// <summary>
    /// Provides a centralized service for managing Azure Service Bus connections, processors, and senders.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This provider serves as a factory for creating and managing various Azure Service Bus entities:
    /// <list type="bullet">
    ///   <item><description>Senders for publishing messages to queues and topics</description></item>
    ///   <item><description>Processors for consuming messages from queues and topics</description></item>
    ///   <item><description>Receivers for receiving messages from queues and topic subscriptions</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The provider automatically manages entity lifecycle, ensuring queues, topics, and subscriptions
    /// exist before attempting to create clients for them. It also implements connection pooling by
    /// caching and reusing client instances.
    /// </para>
    /// <para>
    /// This class implements <see cref="IAsyncDisposable"/> and properly cleans up all Service Bus resources
    /// when disposed. Always use in a using statement or with dependency injection to ensure proper cleanup.
    /// </para>
    /// </remarks>
    public sealed class AmqpConnectionProvider(ServiceBusClient _client, ServiceBusAdministrationClient _adminClient, ILogger<AmqpConnectionProvider> _logger) : IAsyncDisposable
    {
        private readonly ConcurrentDictionary<string, ServiceBusSender> _senders = new();
        private readonly ConcurrentDictionary<string, ServiceBusProcessor> _processors = new();
        private readonly ConcurrentDictionary<string, ServiceBusReceiver> _receivers = new();
        private bool _disposed;

        /// <summary>
        /// Creates a ServiceBusSender for a queue or topic.
        /// </summary>
        /// <param name="entityPath">The path to the queue or topic</param>
        /// <returns>A sender for the specified entity</returns>
        public async Task<ServiceBusSender> CreateSenderAsync(string entityPath, bool isTopic, CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(AmqpConnectionProvider));

            // If we already have a sender for this entity, return it
            if (_senders.TryGetValue(entityPath, out var existingSender))
                return existingSender;

            // Ensure entity exists before creating a sender
            if (isTopic)
            {
                await EnsureTopicExistsAsync(entityPath, cancellationToken);
            }
            else
            {
                await EnsureQueueExistsAsync(entityPath, cancellationToken);
            }

            ServiceBusSender sender = _client.CreateSender(entityPath);

            // Cache the sender
            _senders[entityPath] = sender;

            return sender;
        }


        /// <summary>
        /// Creates a ServiceBusProcessor for a queue.
        /// </summary>
        /// <param name="queueName">The name of the queue</param>
        /// <returns>A processor for the specified queue</returns>
        public async Task<ServiceBusProcessor> CreateQueueProcessorAsync(string queueName, CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(AmqpConnectionProvider));

            // If we already have a processor for this queue, return it
            string key = $"queue:{queueName}";

            if (_processors.TryGetValue(key, out var existingProcessor))
                return existingProcessor;

            // Ensure queue exists
            await EnsureQueueExistsAsync(queueName, cancellationToken);

            ServiceBusProcessor processor = _client.CreateProcessor(queueName);

            // Cache the processor
            _processors[key] = processor;

            return processor;
        }

        /// <summary>
        /// Creates a ServiceBusProcessor for a topic subscription.
        /// </summary>
        /// <param name="topicPath">The path to the topic</param>
        /// <param name="subscriptionName">The name of the subscription</param>
        /// <returns>A processor for the specified topic subscription</returns>
        public async Task<ServiceBusProcessor> CreateTopicProcessorAsync(string topicPath, string subscriptionName, CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(AmqpConnectionProvider));

            // If we already have a processor for this topic subscription, return it
            string key = $"topic:{topicPath}:subscription:{subscriptionName}";

            if (_processors.TryGetValue(key, out var existingProcessor))
                return existingProcessor;

            // Ensure topic and subscription exist

            await EnsureSubscriptionExistsAsync(topicPath, subscriptionName, cancellationToken);

            ServiceBusProcessor processor = _client.CreateProcessor(topicPath, subscriptionName);

            // Cache the processor
            _processors[key] = processor;

            return processor;
        }

        /// <summary>
        /// Ensures that a queue exists, creating it if it doesn't.
        /// </summary>
        /// <param name="queueName">The name of the queue to check or create</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        internal async Task EnsureQueueExistsAsync(string queueName, CancellationToken cancellationToken = default)
        {
            try
            {
                if (await _adminClient.QueueExistsAsync(queueName, cancellationToken))
                {
                    return;
                }

                // First check if a topic with this name exists
                if (await _adminClient.TopicExistsAsync(queueName, cancellationToken))
                {
                    _logger.LogWarning("Cannot create queue '{QueueName}' because a topic with the same name already exists", queueName);
                    return;
                }

                // Check if the queue exists
                if (!await _adminClient.QueueExistsAsync(queueName, cancellationToken))
                {
                    _logger.LogInformation("Creating queue '{QueueName}' as it does not exist", queueName);

                    try
                    {
                        await _adminClient.CreateQueueAsync(queueName, cancellationToken);

                        _logger.LogInformation("Queue '{QueueName}' created successfully", queueName);
                    }
                    catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists)
                    {
                        // Queue was created between our check and create - this is fine
                        _logger.LogInformation(ex, "Queue '{QueueName}' was created by another process", queueName);
                    }
                }
            }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists)
            {
                // Entity already exists with a different type (topic vs queue)
                _logger.LogWarning(ex, "Cannot create queue '{QueueName}'. An entity with this name already exists but has a different type.", queueName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking or creating queue '{QueueName}'", queueName);
                // Don't throw - attempt to continue anyway
            }
        }

        /// <summary>
        /// Ensures that a topic exists for a topic, creating it if it doesn't.
        /// </summary>
        /// <param name="topicPath">The name of the topic</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        internal async Task EnsureTopicExistsAsync(string topicPath, CancellationToken cancellationToken = default)
        {
            // First make sure the topic exists
            // Check if the topic exists
            if (!await _adminClient.TopicExistsAsync(topicPath, cancellationToken))
            {
                _logger.LogInformation("Creating topic '{TopicPath}' as it does not exist", topicPath);

                try
                {
                    await _adminClient.CreateTopicAsync(topicPath, cancellationToken);

                    _logger.LogInformation("Topic '{TopicPath}' created successfully", topicPath);
                }
                catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists)
                {
                    // Topic was created between our check and create - this is fine
                    _logger.LogInformation(ex, "Topic '{TopicPath}' was created by another process", topicPath);
                }
            }
        }


        /// <summary>
        /// Ensures that a subscription exists for a topic, creating it if it doesn't.
        /// </summary>
        /// <param name="topicPath">The name of the topic</param>
        /// <param name="subscriptionName">The name of the subscription to check or create</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        internal async Task EnsureSubscriptionExistsAsync(string topicPath, string subscriptionName, CancellationToken cancellationToken = default)
        {
            try
            {
                // First make sure the topic exists
                // Check if the topic exists
                if (!await _adminClient.TopicExistsAsync(topicPath, cancellationToken))
                {
                    _logger.LogInformation("Creating topic '{TopicPath}' as it does not exist", topicPath);

                    try
                    {
                        await _adminClient.CreateTopicAsync(topicPath, cancellationToken);

                        _logger.LogInformation("Topic '{TopicPath}' created successfully", topicPath);
                    }
                    catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists)
                    {
                        // Topic was created between our check and create - this is fine
                        _logger.LogInformation(ex, "Topic '{TopicPath}' was created by another process", topicPath);
                    }
                }

                if (!await _adminClient.SubscriptionExistsAsync(topicPath, subscriptionName, cancellationToken))
                {
                    // Check if the subscription exists

                    _logger.LogInformation("Creating subscription '{SubscriptionName}' for topic '{TopicPath}' as it does not exist", subscriptionName, topicPath);

                    try
                    {
                        await _adminClient.CreateSubscriptionAsync(topicPath, subscriptionName, cancellationToken);

                        _logger.LogInformation("Subscription '{SubscriptionName}' for topic '{TopicPath}' created successfully", subscriptionName, topicPath);
                    }
                    catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists)
                    {
                        // Subscription was created between our check and create - this is fine
                        _logger.LogInformation(ex, "Subscription '{SubscriptionName}' for topic '{TopicPath}' was created by another process", subscriptionName, topicPath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking or creating subscription '{SubscriptionName}' for topic '{TopicPath}'", subscriptionName, topicPath);
                // Don't throw - attempt to continue anyway
            }
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            // Dispose all senders
            foreach (var sender in _senders.Values)
            {
                try
                {
                    await sender.DisposeAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing sender");
                }
            }
            _senders.Clear();

            // Dispose all processors
            foreach (var processor in _processors.Values)
            {
                try
                {
                    await processor.StopProcessingAsync();
                    await processor.DisposeAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing processor");
                }
            }
            _processors.Clear();

            // Dispose all receivers
            foreach (var receiver in _receivers.Values)
            {
                try
                {
                    await receiver.DisposeAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing receiver");
                }
            }
            _receivers.Clear();

            // Dispose the client
            if (_client != null)
            {
                try
                {
                    await _client.DisposeAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing client");
                }
            }

            _disposed = true;

            GC.SuppressFinalize(this);
        }
    }
}