namespace Flaminco.Keycloak.Models
{
    /// <summary>
    /// Represents the configuration options for Keycloak.
    /// </summary>
    public class KeycloakOptions
    {
        /// <summary>
        /// Gets or sets the authentication URL of the Keycloak server.
        /// </summary>
        public string AuthUrl { get; set; }

        /// <summary>
        /// Gets or sets the realm to be used in Keycloak.
        /// </summary>
        public string Realm { get; set; }

        /// <summary>
        /// Gets or sets the audience for token validation.
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether HTTPS metadata is required.
        /// </summary>
        public bool RequireHttpsMetadata { get; set; }

        /// <summary>
        /// Gets or sets the clock skew for token validation.
        /// </summary>
        public TimeSpan? ClockSkew { get; set; }

        /// <summary>
        /// Gets or sets the claim type for roles.
        /// </summary>
        public string RoleClaimType { get; set; } = "role";

        /// <summary>
        /// Gets or sets the claim type for the user's name.
        /// </summary>
        public string NameClaimType { get; set; } = "name";

        /// <summary>
        /// Gets or sets a value indicating whether the token should be saved.
        /// </summary>
        public bool SaveToken { get; set; }

        /// <summary>
        /// Gets or sets the credentials for Keycloak.
        /// </summary>
        public KeycloakCredentials Credentials { get; set; }


        /// <summary>
        /// Validates the Keycloak options.
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrEmpty(AuthUrl))
            {
                throw new ArgumentException("AuthUrl must be set in the Keycloak options.");
            }
            if (string.IsNullOrEmpty(Realm))
            {
                throw new ArgumentException("Realm must be set in the Keycloak options.");
            }
            if (string.IsNullOrEmpty(Audience))
            {
                throw new ArgumentException("Audience must be set in the Keycloak options.");
            }
            if (Credentials == null)
            {
                throw new ArgumentException("Credentials must be set in the Keycloak options.");
            }
            if (string.IsNullOrEmpty(Credentials.ClientId))
            {
                throw new ArgumentException("ClientId must be set in the Keycloak credentials.");
            }
            if (string.IsNullOrEmpty(Credentials.ClientSecret))
            {
                throw new ArgumentException("ClientSecret must be set in the Keycloak credentials.");
            }
        }
    }

    /// <summary>
    /// Represents the credentials for Keycloak.
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
