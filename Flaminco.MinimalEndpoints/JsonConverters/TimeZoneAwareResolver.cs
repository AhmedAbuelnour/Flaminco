using Flaminco.MinimalEndpoints.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Flaminco.MinimalEndpoints.JsonConverters
{
    public class TimeZoneAwareResolver : DefaultJsonTypeInfoResolver
    {
        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            // Let the base resolver build all of the usual type/prop metadata:
            JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

            // Now inspect every property in that type. If the CLR property has [TimeZoneAware],
            // attach our converter to it.
            foreach (JsonPropertyInfo propInfo in jsonTypeInfo.Properties)
            {
                // propInfo.AttributeProvider can see all CLR attributes on that property/field.
                if (propInfo.AttributeProvider?.GetCustomAttributes(typeof(TimeZoneAwareAttribute), inherit: true).Length > 0)
                {
                    if (propInfo.PropertyType == typeof(DateTimeOffset))
                    {
                        propInfo.CustomConverter = new TimeZoneAwareDateTimeOffsetConverter();
                    }
                    if (propInfo.PropertyType == typeof(DateTimeOffset?))
                    {
                        propInfo.CustomConverter = new TimeZoneAwareNullableDateTimeOffsetConverter();
                    }
                }
            }

            return jsonTypeInfo;
        }
    }

}
