using Amqp;
using Flaminco.RabbitMQ.AMQP.Events;
using Flaminco.RabbitMQ.AMQP.Options;
using MediatR;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Flaminco.RabbitMQ.AMQP.Abstractions
{
    /// <summary>
    /// Represents an abstract base class for consuming messages from a message queue.
    /// </summary>
    public abstract class MessageConsumer : IAsyncDisposable
    {
        private readonly IOptions<AMQPClientSettings> _addressSettings;
        private readonly IPublisher _publisher;
        private Connection? _connection;
        private Session? _session;
        private ReceiverLink? _receiver;

        /// <summary>
        /// Represents an abstract base class for consuming messages from a message queue.
        /// </summary>
        /// <param name="addressSettings">The address settings used to configure the connection to the message broker.</param>
        /// <param name="publisher">The event publisher for notifying when a message is received or when a fault occurs.</param>
        protected MessageConsumer(IOptions<AMQPClientSettings> addressSettings, IPublisher publisher)
        {
            _addressSettings = addressSettings;
            _publisher = publisher;
        }

        private bool IsClosed { get => (_connection?.IsClosed ?? true) || (_session?.IsClosed ?? true); }

        /// <summary>
        /// Provides default serialization options for JSON, configured with web defaults.
        /// </summary>
        private readonly JsonSerializerOptions DefaultSerializeOptions = new(JsonSerializerDefaults.Web);

        /// <summary>
        /// Gets the unique name of the message consumer, used to identify the consumer when consuming messages from the queue.
        /// </summary>
        protected abstract string Name { get; }


        /// <summary>
        /// Gets the name of the queue from which messages are consumed.
        /// </summary>
        protected abstract string Queue { get; }


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
        public async Task ConsumeAsync<TMessage>(CancellationToken cancellationToken = default)
        {
            if (IsClosed)
            {
                await ConnectAsync();

                _receiver = new(_session, Name, Queue);
            }

            Message message = await _receiver!.ReceiveAsync(TimeOut);

            if (message?.Body is byte[] body && DeserializeSafely<TMessage>(body) is TMessage receivedMessage)
            {
                _receiver.Accept(message);

                await _publisher.Publish(new MessageReceivedEvent<TMessage>
                {
                    Message = receivedMessage
                }, cancellationToken);
            }
            else
            {
                _receiver.Reject(message);

                await _publisher.Publish(new MessageFaultEvent<TMessage>
                {
                    Name = Name,
                    Queue = Queue,
                    Message = message
                }, cancellationToken);
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

        private T? DeserializeSafely<T>(byte[] body)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(body, options: DefaultSerializeOptions);
            }
            catch
            {
                return default;
            }
        }
    }

}
