namespace Flaminco.RabbitMQ.AMQP.HostedServices
{
    using Amqp;
    using Flaminco.RabbitMQ.AMQP.Abstractions;
    using Flaminco.RabbitMQ.AMQP.Attributes;
    using Flaminco.RabbitMQ.AMQP.Services;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Hosted service responsible for managing AMQP consumers.
    /// </summary>
    internal sealed class AmqpConsumerHostedService(IServiceProvider serviceProvider,
                                                    AmqpConnectionProvider connectionProvider,
                                                    ILogger<AmqpConsumerHostedService> logger) : IHostedService, IDisposable
    {
        private readonly List<(ReceiverLink Receiver, CancellationTokenSource CancellationSource)> _receivers = [];
        private bool _disposed;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting AMQP consumers");

            foreach (Type consumerType in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => t.GetCustomAttributes<QueueConsumerAttribute>().Any()))
            {
                if (consumerType.GetCustomAttribute<QueueConsumerAttribute>() is QueueConsumerAttribute queueConsumer)
                {
                    try
                    {
                        logger.LogInformation("Starting consumer for queue {Queue}", queueConsumer.Queue);

                        // Get a session for this queue
                        Session session = await connectionProvider.GetSessionAsync(queueConsumer.Queue);

                        // Create a cancellation token source for this consumer
                        CancellationTokenSource consumerCts = new();

                        // Create a receiver for the queue
                        ReceiverLink receiver = new(session, $"receiver-{Guid.NewGuid()}", queueConsumer.Queue);

                        // Get the message type by finding the generic type parameter of MessageConsumer<T>
                        Type? messageConsumerBaseType = consumerType.BaseType;

                        while (messageConsumerBaseType != null && (!messageConsumerBaseType.IsGenericType ||
                               messageConsumerBaseType.GetGenericTypeDefinition() != typeof(MessageConsumer<>)))
                        {
                            messageConsumerBaseType = messageConsumerBaseType.BaseType;
                        }

                        if (messageConsumerBaseType == null)
                        {
                            logger.LogError("Could not find MessageConsumer<T> base type for consumer {Consumer}", consumerType.Name);

                            continue;
                        }

                        // Configure the receiver to handle messages
                        receiver.Start(
                            credit: 20,  // Prefetch count
                            onMessage: async (receiver, message) =>
                            {
                                try
                                {
                                    using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();

                                    object consumer = scope.ServiceProvider.GetRequiredService(consumerType);

                                    if (consumerType.BaseType?.GetMethod("ProcessMessageAsync", BindingFlags.NonPublic | BindingFlags.Instance) is MethodInfo processMethod)
                                    {
                                        if (processMethod.Invoke(consumer, [message, consumerCts.Token]) is Task processMessageAsync)
                                        {
                                            await processMessageAsync;

                                            // Accept the message
                                            receiver.Accept(message);
                                        }
                                    }
                                    else
                                    {
                                        logger.LogError("ProcessMessageAsync method not found on consumer {Consumer}", consumerType.Name);

                                        receiver.Reject(message);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError(ex, "Error processing message on queue {Queue}", queueConsumer.Queue);

                                    receiver.Reject(message);
                                }
                            });

                        // Store the receiver and cancellation token source
                        _receivers.Add((receiver, consumerCts));
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error setting up consumer for queue {Queue}", queueConsumer.Queue);
                    }
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Stopping AMQP consumers");

            foreach (var (receiver, cts) in _receivers)
            {
                try
                {
                    // Cancel any ongoing processing
                    cts.Cancel();

                    // Close the receiver
                    await receiver.CloseAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error stopping consumer");
                }
            }

            _receivers.Clear();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            foreach (var (_, cts) in _receivers)
            {
                cts.Dispose();
            }

            _disposed = true;

            GC.SuppressFinalize(this);
        }
    }
}
