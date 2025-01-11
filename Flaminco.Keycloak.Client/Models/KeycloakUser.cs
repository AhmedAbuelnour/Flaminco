using System.Text.Json.Serialization;

namespace Flaminco.Keycloak.Client.Models
{
    /// <summary>
    /// Represents a user in Keycloak.
    /// </summary>
    public class KeycloakUser
    {
        /// <summary>
        /// the ID of the user.
        /// </summary>
        [JsonPropertyName("id")] public string Id { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the user was created.
        /// </summary>
        [JsonPropertyName("createdTimestamp")] public long CreatedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the username of the user.
        /// </summary>
        [JsonPropertyName("username")] public string Username { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is enabled.
        /// </summary>
        [JsonPropertyName("enabled")] public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the first name of the user.
        /// </summary>
        [JsonPropertyName("firstName")] public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the user.
        /// </summary>
        [JsonPropertyName("lastName")] public string? LastName { get; set; }

        /// <summary>
        /// Gets or sets the email of the user.
        /// </summary>
        [JsonPropertyName("email")] public string Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user's email is verified.
        /// </summary>
        [JsonPropertyName("emailVerified")] public bool EmailVerified { get; set; }

        /// <summary>
        /// Gets or sets the attributes of the user.
        /// </summary>
        [JsonPropertyName("attributes")] public Dictionary<string, IEnumerable<object>>? Attributes { get; set; }

        /// <summary>
        /// Gets the full name of the user.
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// Gets the attribute value of the specified type.
        /// </summary>
        /// <typeparam name="T">The type to convert the attribute value to.</typeparam>
        /// <param name="type">The attribute type to retrieve.</param>
        /// <returns>The attribute value converted to the specified type, or the default value if not found.</returns>
        /// <exception cref="ArgumentException">Thrown when the type parameter is null or empty.</exception>
        /// <exception cref="InvalidCastException">Thrown when the attribute value cannot be converted to the specified type.</exception>
        /// <exception cref="FormatException">Thrown when the format of the attribute value is invalid for the specified type conversion.</exception>
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
