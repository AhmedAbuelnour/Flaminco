using Flaminco.MinimalEndpoints.Abstractions;
using System.Text.Json;

namespace Flaminco.MinimalEndpoints.Implementations
{
    internal static class EventSerialization
    {
        private sealed record EventEnvelope(string Type, string Payload);

        public static string Serialize(IDomainEvent domainEvent)
        {
            var runtimeType = domainEvent.GetType();
            var payload = JsonSerializer.Serialize(domainEvent, runtimeType);

            var envelope = new EventEnvelope(
                runtimeType.AssemblyQualifiedName ?? runtimeType.FullName ?? runtimeType.Name,
                payload);

            return JsonSerializer.Serialize(envelope);
        }

        public static IDomainEvent? Deserialize(string raw)
        {
            EventEnvelope? envelope;

            try
            {
                envelope = JsonSerializer.Deserialize<EventEnvelope>(raw);
            }
            catch
            {
                return null;
            }

            if (envelope is null || string.IsNullOrWhiteSpace(envelope.Type) || string.IsNullOrWhiteSpace(envelope.Payload))
            {
                return null;
            }

            var runtimeType = Type.GetType(envelope.Type, throwOnError: false);
            if (runtimeType is null || !typeof(IDomainEvent).IsAssignableFrom(runtimeType))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize(envelope.Payload, runtimeType) as IDomainEvent;
            }
            catch
            {
                return null;
            }
        }
    }
}

