using System.Text.Json;
using System.Text.Json.Serialization;

namespace Flaminco.MinimalEndpoints.JsonConverters
{
    internal sealed class TimeZoneAwareNullableDateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
    {
        private readonly TimeZoneAwareDateTimeOffsetConverter _inner = new();

        public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            // Delegate to the non‐nullable converter for actual parsing.
            return _inner.Read(ref reader, typeof(DateTimeOffset), options);
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();

                return;
            }

            // Delegate the non‐null case to the existing converter.
            _inner.Write(writer, value.Value, options);
        }
    }

}
