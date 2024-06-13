using System.Text.Json.Serialization;

namespace Flaminco.Keycloak.Models
{
    public class UserAccount
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("createdTimestamp")] public long CreatedTimestamp { get; set; }
        [JsonPropertyName("username")] public string Username { get; set; }
        [JsonPropertyName("enabled")] public bool Enabled { get; set; }
        [JsonPropertyName("firstName")] public string FirstName { get; set; }
        [JsonPropertyName("lastName")] public string LastName { get; set; }
        [JsonPropertyName("email")] public string Email { get; set; }
        [JsonPropertyName("emailVerified")] public bool EmailVerified { get; set; }
        [JsonPropertyName("attributes")] public Dictionary<string, IEnumerable<object>>? Attributes { get; set; }

        public string FullName => $"{this.FirstName} {this.LastName}";

        public T? GetAttribute<T>(string type)
        {
            // Check if 'type' is null or empty to avoid errors in dictionary lookup
            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentException("Type parameter cannot be null or empty.", nameof(type));
            }

            // Try to find the attribute by the given type
            if ((Attributes?.TryGetValue(type, out IEnumerable<object>? value) ?? false) && value.FirstOrDefault() is object firstValue)
            {
                try
                {
                    // Attempt to convert the attribute value to the requested type 'T'
                    return (T)Convert.ChangeType(firstValue, typeof(T));
                }
                catch (InvalidCastException)
                {
                    // Throw a more specific exception if conversion fails
                    throw new InvalidCastException($"Cannot convert attribute value to type {typeof(T).Name}.");
                }
                catch (FormatException)
                {
                    // Handle format errors specifically for cases where conversion fails due to format issues
                    throw new FormatException($"Format is invalid for type {typeof(T).Name} conversion.");
                }
            }

            // Return default value of 'T' if the attribute is not found or if the value is null
            return default;
        }
    }
}
