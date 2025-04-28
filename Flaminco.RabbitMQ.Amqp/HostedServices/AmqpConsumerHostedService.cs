namespace Flaminco.RabbitMQ.AMQP.HostedServices
{
    using Flaminco.RabbitMQ.AMQP.Abstractions;
    using Flaminco.RabbitMQ.AMQP.Attributes;
    using Flaminco.RabbitMQ.AMQP.Services;
    using global::RabbitMQ.Client;
    using global::RabbitMQ.Client.Events;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System.Reflection;

    /// <summary>
    /// Hosted service responsible for managing RabbitMQ consumers.
    /// </summary>
    internal sealed class AmqpConsumerHostedService(IServiceProvider serviceProvider,
                                              AmqpConnectionProvider connectionProvider,
                                              ILogger<AmqpConsumerHostedService> logger) : IHostedService, IAsyncDisposable
    {
        private readonly List<(IChannel Channel, AsyncEventingBasicConsumer Consumer, CancellationTokenSource CancellationSource, string ConsumerTag)> _consumers = [];
        private bool _disposed;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting RabbitMQ consumers");

            foreach (Type consumerType in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => t.GetCustomAttributes<QueueConsumerAttribute>().Any()))
            {
                if (consumerType.GetCustomAttribute<QueueConsumerAttribute>() is QueueConsumerAttribute queueConsumer)
                {
                    try
                    {
                        logger.LogInformation("Starting consumer for queue {Queue}", queueConsumer.Queue);

                        // Create a channel for this queue
                        IChannel channel = await connectionProvider.GetChannelForQueueAsync(queueConsumer.Queue, cancellationToken);

                        // Create a cancellation token source for this consumer
                        CancellationTokenSource consumerCts = new();

                        // Ensure the queue exists by declaring it (this will create it if it doesn't exist)
                        await channel.QueueDeclareAsync(
                            queue: queueConsumer.Queue,
                            durable: true,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null,
                            cancellationToken: cancellationToken);

                        // Set QoS / prefetch count
                        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 20, global: false, cancellationToken);

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

                        // Create an async consumer
                        AsyncEventingBasicConsumer consumer = new(channel);

                        // Set up the received event handler
                        consumer.ReceivedAsync += async (_, args) =>
                        {
                            try
                            {
                                AsyncServiceScope scope = serviceProvider.CreateAsyncScope();

                                object consumerInstance = scope.ServiceProvider.GetRequiredService(consumerType);

                                if (consumerType.BaseType?.GetMethod("ProcessMessageAsync", BindingFlags.NonPublic | BindingFlags.Instance) is MethodInfo processMethod)
                                {
                                    if (processMethod.Invoke(consumerInstance, [args, consumerCts.Token]) is Task processMessageTask)
                                    {
                                        await processMessageTask;

                                        // Acknowledge the message
                                        await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken);
                                    }
                                }
                                else
                                {
                                    logger.LogError("ProcessMessageAsync method not found on consumer {Consumer}", consumerType.Name);

                                    await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: true, cancellationToken);
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Error processing message on queue {Queue}", queueConsumer.Queue);
                                // Reject the message but allow it to be requeued
                                await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: true, cancellationToken);
                            }
                        };

                        // Start consuming from the queue
                        string consumerTag = await channel.BasicConsumeAsync(queue: queueConsumer.Queue, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);

                        // Store the channel, consumer, and cancellation token source
                        _consumers.Add((channel, consumer, consumerCts, consumerTag));
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
            logger.LogInformation("Stopping RabbitMQ consumers");

            foreach ((IChannel channel, AsyncEventingBasicConsumer _, CancellationTokenSource cts, string consumerTag) in _consumers)
            {
                try
                {
                    // Cancel any ongoing processing
                    await cts.CancelAsync();

                    // Cancel the consumer
                    if (channel.IsOpen)
                    {
                        await channel.BasicCancelAsync(consumerTag, cancellationToken: cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error stopping consumer");
                }
            }

            _consumers.Clear();
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            foreach ((IChannel channel, AsyncEventingBasicConsumer _, CancellationTokenSource cts, string _) in _consumers)
            {
                try
                {
                    await channel.CloseAsync();

                    channel.Dispose();
                }
                catch
                {
                }
                cts.Dispose();
            }

            _disposed = true;

            GC.SuppressFinalize(this);
        }
    }
}
