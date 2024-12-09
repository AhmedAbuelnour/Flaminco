namespace Flaminco.Keycloak.Models
{
    /// <summary>
    /// Represents the JWKS (JSON Web Key Set) options for Keycloak.
    /// This class provides properties to configure the JWKS client, including the backchannel HTTP handler and refresh intervals.
    /// The BackchannelHttpHandler property allows customization of the HTTP handler used for backchannel communication.
    /// The AutomaticRefreshInterval property specifies the time interval after which the JWKS client will automatically refresh its configuration.
    /// The RefreshInterval property defines the minimum time interval that must pass before the JWKS client can request a new configuration.
    /// </summary>
    public class JwksOptions
    {
        /// <summary>
        /// Gets or sets the Server Certificate Custom Validation Callback for JwksClient.
        /// This property allows customization of the HTTP handler used for backchannel communication with the JWKS endpoint.
        /// </summary>
        public HttpClientHandler BackchannelHttpHandler { get; set; } = default!;

        /// <summary>
        /// 12 hours is the default time interval that afterwards, <see cref="GetBaseConfigurationAsync(CancellationToken)"/> will obtain new configuration.
        /// This property specifies the time interval after which the JwksRetriever will automatically refresh its configuration.
        /// ensures the configuration is refreshed periodically, regardless of traffic or errors.
        /// </summary>
        public TimeSpan? AutomaticRefreshInterval { get; set; }

        /// <summary>
        /// 5 minutes is the default time interval that must pass for <see cref="RequestRefresh"/> to obtain a new configuration.
        /// This property defines the minimum time interval that must pass before the JwksRetriever can request a new configuration.
        /// ensures the configuration is refreshed but only if token validation failures and the last refresh to be passed the refresh interval.
        /// </summary>
        public TimeSpan? RefreshInterval { get; set; }
    }
}
