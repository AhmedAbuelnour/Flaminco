using Flaminco.Keycloak.Client.Models;

namespace Flaminco.Keycloak.Authentication.Models
{
    /// <summary>
    /// Represents the configuration options for Keycloak, including server URLs, realm, token validation settings, and credentials.
    /// This class provides properties to configure the Keycloak server's base URL, realm, issuer, audience, and other settings required for authentication and token validation.
    /// It also includes methods to construct metadata and certificate URLs based on the provided base URL and realm.
    /// Additionally, it validates the required configuration options to ensure they are set correctly.
    /// </summary>
    public class KeycloakClientOptions
    {
        /// <summary>
        /// Gets or sets the authentication URL of the Keycloak server.
        /// </summary>
        public string KeycloakBaseUrl { get; set; } = default!;


        /// <summary>
        /// Gets or sets the realm to be used in Keycloak.
        /// </summary>
        public string Realm { get; set; } = default!;


        /// <summary>
        /// Gets or sets the credentials for Keycloak.
        /// </summary>
        public KeycloakCredentials Credentials { get; set; } = default!;


        public string AdminBaseUrl { get => $"{KeycloakBaseUrl}/admin/realms/{Realm}"; }


        /// <summary>
        /// Validates the Keycloak options.
        /// </summary>
        public void Validate()
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(KeycloakBaseUrl, nameof(KeycloakBaseUrl));
            ArgumentException.ThrowIfNullOrWhiteSpace(Realm, nameof(Realm));
            ArgumentException.ThrowIfNullOrWhiteSpace(Credentials?.ClientId, nameof(KeycloakCredentials.ClientId));
            ArgumentException.ThrowIfNullOrWhiteSpace(Credentials?.ClientSecret, nameof(KeycloakCredentials.ClientSecret));
        }
    }
}
