using Flaminco.MinimalEndpoints.Contexts;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Flaminco.MinimalEndpoints.JsonConverters
{
    internal sealed class TimeZoneAwareDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Reading stays “raw” (UTC). If you need to map incoming values back to UTC → local, 
            // you could check _timeZoneId.Value here and do ConvertTimeToUtc, etc.
            return reader.GetDateTimeOffset();
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            DateTimeOffset outputValue = TimeZoneInfo.ConvertTime(value, TimeZoneContext.Zone);

            // Always write with offset (“zzz”) so the consumer sees the actual offset in the string
            writer.WriteStringValue(outputValue.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffzzz"));
        }
    }

}
