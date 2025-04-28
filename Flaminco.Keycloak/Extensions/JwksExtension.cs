using Flaminco.Keycloak.Authentication.JwtBearer.Retrievers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Flaminco.Keycloak.Authentication.JwtBearer.Extensions
{
    internal static class JwksExtension
    {
        internal static void SetJwksOptions(this JwtBearerOptions options, string certsAddress)
        {
            HttpClient httpClient = new(options.BackchannelHttpHandler!)
            {
                Timeout = options.BackchannelTimeout,
                MaxResponseContentBufferSize = 1024 * 1024 * 10,
            };

            options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(options.MetadataAddress, new JwksRetriever(certsAddress), new HttpDocumentRetriever(httpClient)
            {
                RequireHttps = options.RequireHttpsMetadata,
            })
            {
                AutomaticRefreshInterval = options.AutomaticRefreshInterval,
                RefreshInterval = options.RefreshInterval,
            };
        }
    }

}
