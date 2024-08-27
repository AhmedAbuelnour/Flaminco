﻿using Amqp;
using Flaminco.RabbitMQ.AMQP.Options;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Flaminco.RabbitMQ.AMQP.Abstractions
{
    /// <summary>
    /// Represents an abstract base class for publishing messages to a message queue.
    /// </summary>
    /// <param name="_addressSettings">The address settings used to configure the connection to the message broker.</param>
    public abstract class MessagePublisher(IOptions<AddressSettings> _addressSettings) : IAsyncDisposable
    {
        private Connection? _connection;
        private Session? _session;
        private bool IsClosed { get => (_connection?.IsClosed ?? true) || (_session?.IsClosed ?? true); }

        /// <summary>
        /// Provides default serialization options for JSON, configured with web defaults.
        /// </summary>
        private readonly JsonSerializerOptions DefaultSerializeOptions = new(JsonSerializerDefaults.Web);
        /// <summary>
        /// Retrieves the key for the message asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation that returns the key as a string.</returns>
        protected abstract ValueTask<string> GetKeyAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the queues name for the message asynchronously. it supports sending same message to multiple queues so different consumers can receive the same message.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation that returns the queue name as a string.</returns>
        protected abstract ValueTask<string[]> GetQueuesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// The time to wait for the task to complete for each message in a queue, default is 60 seconds
        /// </summary>
        public virtual TimeSpan TimeOut { get; set; } = TimeSpan.FromSeconds(60);

        private async Task ConnectAsync()
        {
            ConnectionFactory connectionFactory = new();

            _connection = await connectionFactory.CreateAsync(new Address(_addressSettings.Value.ConnectionString));

            _session = new Session(_connection);
        }

        /// <summary>
        /// Publishes a message asynchronously to the configured queue.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message to publish.</typeparam>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous publish operation.</returns>
        public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        {
            if (IsClosed)
            {
                await ConnectAsync();
            }

            string key = await GetKeyAsync(cancellationToken);

            Message AMQPMessage = new(JsonSerializer.SerializeToUtf8Bytes(message, options: DefaultSerializeOptions));

            foreach (string queue in await GetQueuesAsync(cancellationToken))
            {
                SenderLink sender = new(_session, key, queue);

                await sender.SendAsync(AMQPMessage, TimeOut);

                await sender.CloseAsync();
            }
        }
        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (_session is not null)
            {
                await _session.CloseAsync();
            }

            if (_connection is not null)
            {
                await _connection.CloseAsync();
            }
        }
    }
}