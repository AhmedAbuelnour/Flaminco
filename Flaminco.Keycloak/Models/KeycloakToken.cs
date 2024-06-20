using System.Text.Json.Serialization;

namespace Flaminco.Keycloak.Models
{
    /// <summary>
    /// Represents a token response from Keycloak.
    /// </summary>
    public class KeycloakToken
    {
        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        [JsonPropertyName("access_token")] public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds until the access token expires.
        /// </summary>
        [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds until the refresh token expires.
        /// </summary>
        [JsonPropertyName("refresh_expires_in")] public int? RefreshExpiresIn { get; set; }

        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        [JsonPropertyName("refresh_token")] public string? RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the type of the token.
        /// </summary>
        [JsonPropertyName("token_type")] public string TokenType { get; set; }

        /// <summary>
        /// Gets or sets the not-before policy value.
        /// </summary>
        [JsonPropertyName("not-before-policy")] public int? NotBeforePolicy { get; set; }

        /// <summary>
        /// Gets or sets the session state.
        /// </summary>
        [JsonPropertyName("session_state")] public string? SessionState { get; set; }

        /// <summary>
        /// Gets or sets the scope of the token.
        /// </summary>
        [JsonPropertyName("scope")] public string? Scope { get; set; }
    }
}
