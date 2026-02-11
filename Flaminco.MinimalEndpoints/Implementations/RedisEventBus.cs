using Flaminco.MinimalEndpoints.Abstractions;
using Flaminco.MinimalEndpoints.Options;
using StackExchange.Redis;

namespace Flaminco.MinimalEndpoints.Implementations
{
    /// <summary>
    /// Redis-backed event bus implementation using Redis Lists (RPUSH + BLPOP).
    /// </summary>
    public sealed class RedisEventBus : IEventBus, IAsyncDisposable
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IDatabase _database;
        private readonly string _queueKey;
        private readonly int _popTimeoutSeconds;
        private readonly bool _ownsConnectionMultiplexer;
        private bool _disposed;

        public RedisEventBus(RedisEventBusOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            if (options.ConnectionMultiplexer is not null)
            {
                _connectionMultiplexer = options.ConnectionMultiplexer;
                _ownsConnectionMultiplexer = false;
            }
            else
            {
                _connectionMultiplexer = ConnectionMultiplexer.Connect(options.ConnectionString);
                _ownsConnectionMultiplexer = true;
            }

            _database = _connectionMultiplexer.GetDatabase();
            _queueKey = string.IsNullOrWhiteSpace(options.QueueKey) ? "domain-events" : options.QueueKey;
            _popTimeoutSeconds = options.PopTimeoutSeconds <= 0 ? 5 : options.PopTimeoutSeconds;
        }

        public async ValueTask Publish<TDomainEvent>(TDomainEvent domainEvent, CancellationToken cancellationToken = default)
            where TDomainEvent : IDomainEvent
        {
            cancellationToken.ThrowIfCancellationRequested();

            var raw = EventSerialization.Serialize(domainEvent);

            await _database.ListRightPushAsync(_queueKey, raw).ConfigureAwait(false);
        }

        public async IAsyncEnumerable<IDomainEvent> ReadAllAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested && !_disposed)
            {
                RedisResult raw;

                try
                {
                    raw = await _database.ExecuteAsync(
                        "BLPOP",
                        [_queueKey, _popTimeoutSeconds]).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is RedisConnectionException || ex is RedisTimeoutException)
                {

                    await Task.Delay(500, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                if (raw.IsNull)
                {
                    continue;
                }

                // BLPOP returns array: [key, value]
                var arr = (RedisResult[]?)raw;
                if (arr is null || arr.Length < 2 || arr[1].IsNull)
                {
                    continue;
                }

                var payload = (string?)arr[1];
                if (string.IsNullOrWhiteSpace(payload))
                {
                    continue;
                }

                var domainEvent = EventSerialization.Deserialize(payload);
                if (domainEvent is not null)
                {
                    yield return domainEvent;
                }
            }
        }

        public ValueTask DisposeAsync()
        {
            _disposed = true;

            if (_ownsConnectionMultiplexer)
            {
                _connectionMultiplexer.Dispose();
            }

            return ValueTask.CompletedTask;
        }
    }
}

