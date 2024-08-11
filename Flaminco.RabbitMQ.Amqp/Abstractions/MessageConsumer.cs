using Amqp;
using Flaminco.RabbitMQ.AMQP.Options;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Flaminco.RabbitMQ.AMQP.Abstractions
{
    /// <summary>
    /// Represents an abstract base class for consuming messages from a message queue.
    /// </summary>
    /// <param name="_addressSettings">The address settings used to configure the connection to the message broker.</param>
    public abstract class MessageConsumer(IOptions<AddressSettings> _addressSettings) : IAsyncDisposable
    {
        private Connection? _connection;
        private Session? _session;
        private ReceiverLink? _receiver;

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
        /// Retrieves the queue name for the message asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation that returns the queue name as a string.</returns>
        protected abstract ValueTask<string> GetQueueAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// The time to wait for a message, default is Timeout.InfiniteTimeSpan to wait until it receives a message
        /// </summary>
        public virtual TimeSpan TimeOut { get; set; } = Timeout.InfiniteTimeSpan;


        private async Task ConnectAsync()
        {
            ConnectionFactory connectionFactory = new();

            _connection = await connectionFactory.CreateAsync(new Address(_addressSettings.Value.ConnectionString));

            _session = new Session(_connection);
        }

        /// <summary>
        /// Receives a message asynchronously.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message to consume.</typeparam>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation that returns the consumed message, or <c>null</c> if the message could not be consumed.</returns>
        public async Task<TMessage?> ConsumeAsync<TMessage>(CancellationToken cancellationToken = default)
        {
            if (IsClosed)
            {
                await ConnectAsync();

                _receiver = new(_session, await GetKeyAsync(cancellationToken), await GetQueueAsync(cancellationToken));
            }

            Message message = await _receiver!.ReceiveAsync(TimeOut);

            if (message?.Body is byte[] body)
            {
                _receiver.Accept(message);

                return JsonSerializer.Deserialize<TMessage>(body, options: DefaultSerializeOptions);
            }
            else
            {
                _receiver.Reject(message);

                return default;
            }
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (_receiver is not null)
            {
                await _receiver.CloseAsync();
            }

            if (_session is not null)
            {
                await _session!.CloseAsync();
            }

            if (_connection is not null)
            {
                await _connection!.CloseAsync();
            }
        }
    }

}
