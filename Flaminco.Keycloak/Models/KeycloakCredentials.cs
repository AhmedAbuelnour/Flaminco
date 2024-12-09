namespace Flaminco.Keycloak.Models
{
    /// <summary>
    /// Represents the credentials for Keycloak, including client ID and client secret.
    /// This class provides properties to configure the client credentials required for authentication with the Keycloak server.
    /// The ClientId property represents the unique identifier for the client, while the ClientSecret property holds the secret key used for client authentication.
    /// These credentials are essential for obtaining tokens and accessing protected resources in Keycloak.
    /// </summary>
    public class KeycloakCredentials
    {
        /// <summary>
        /// Gets or sets the client secret.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the client ID.
        /// </summary>
        public string ClientId { get; set; }
    }
}
