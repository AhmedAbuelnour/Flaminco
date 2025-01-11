namespace Flaminco.Keycloak.Authentication.JwtBearer.Models
{
    /// <summary>
    /// Represents the configuration options for Keycloak, including server URLs, realm, token validation settings, and credentials.
    /// This class provides properties to configure the Keycloak server's base URL, realm, issuer, audience, and other settings required for authentication and token validation.
    /// It also includes methods to construct metadata and certificate URLs based on the provided base URL and realm.
    /// Additionally, it validates the required configuration options to ensure they are set correctly.
    /// </summary>
    public class KeycloakOptions
    {
        /// <summary>
        /// Gets or sets the authentication URL of the Keycloak server.
        /// </summary>
        public string KeycloakBaseUrl { get; set; } = default!;

        /// <summary>
        /// Gets the Authority URL of the Keycloak server.
        /// </summary>
        public string Authority { get => $"{KeycloakBaseUrl}/realms/{Realm}"; }

        /// <summary>
        /// Gets the MetadataAddress URL of the Keycloak server.
        /// </summary>
        public string MetadataAddress { get => $"{KeycloakBaseUrl}/realms/{Realm}/.well-known/openid-configuration"; }

        /// <summary>
        /// Gets the certs URL of the Keycloak certs.
        /// </summary>
        public string CertsAddress { get => $"{KeycloakBaseUrl}/realms/{Realm}/protocol/openid-connect/certs"; }

        /// <summary>
        /// Gets or sets the realm to be used in Keycloak.
        /// </summary>
        public string Realm { get; set; } = default!;

        /// <summary>
        /// Gets or sets the Issuer for token validation.
        /// </summary>
        public string Issuer { get; set; } = default!;

        /// <summary>
        /// Gets or sets the audience for token validation.
        /// </summary>
        public string Audience { get; set; } = default!;

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
        /// Gets or sets the JWKS options for Keycloak.
        /// </summary>
        public JwksOptions JwksOption { get; set; } = default!;

        /// <summary>
        /// Validates the Keycloak options.
        /// </summary>
        public void Validate()
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(KeycloakBaseUrl, nameof(KeycloakBaseUrl));
            ArgumentException.ThrowIfNullOrWhiteSpace(Realm, nameof(Realm));
            ArgumentException.ThrowIfNullOrWhiteSpace(Audience, nameof(Audience));
            ArgumentException.ThrowIfNullOrWhiteSpace(Issuer, nameof(Issuer));
            ArgumentNullException.ThrowIfNull(JwksOption?.BackchannelHttpHandler, nameof(JwksOptions.BackchannelHttpHandler));
        }
    }
}
