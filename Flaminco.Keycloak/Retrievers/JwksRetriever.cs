using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Flaminco.Keycloak.Retrievers
{
    internal sealed class JwksRetriever(string _certsAddress) : IConfigurationRetriever<OpenIdConnectConfiguration>
    {
        public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(string address, IDocumentRetriever retriever, CancellationToken cancel)
        {
            ArgumentNullException.ThrowIfNull(retriever);

            // Fetch and merge the metadata and JWKS

            Task<OpenIdConnectConfiguration> fetchMetadataTask = FetchMetadataAsync(address, retriever, cancel);

            Task<JsonWebKeySet> fetchJwksTask = FetchJwksAsync(_certsAddress, retriever, cancel);

            await Task.WhenAll(fetchMetadataTask, fetchJwksTask).ConfigureAwait(false);

            OpenIdConnectConfiguration metadata = await fetchMetadataTask.ConfigureAwait(false);

            JsonWebKeySet jwks = await fetchJwksTask.ConfigureAwait(false);

            // Merge JWKS into metadata
            metadata.JsonWebKeySet = jwks;
            metadata.JwksUri = _certsAddress;

            // Add JWKS signing keys to metadata
            foreach (SecurityKey? securityKey in jwks.GetSigningKeys())
            {
                metadata.SigningKeys.Add(securityKey);
            }

            return metadata;
        }

        /// <summary>
        /// Fetches the OpenID Connect metadata from the specified address.
        /// </summary>
        private static async Task<OpenIdConnectConfiguration> FetchMetadataAsync(string address, IDocumentRetriever retriever, CancellationToken cancel)
        {
            string metadataDoc = await retriever.GetDocumentAsync(address, cancel).ConfigureAwait(false);

            return OpenIdConnectConfiguration.Create(metadataDoc);
        }

        /// <summary>
        /// Fetches the JSON Web Key Set (JWKS) from the specified address.
        /// </summary>
        private static async Task<JsonWebKeySet> FetchJwksAsync(string certsAddress, IDocumentRetriever retriever, CancellationToken cancel)
        {
            string jwksDoc = await retriever.GetDocumentAsync(certsAddress, cancel).ConfigureAwait(false);

            return new JsonWebKeySet(jwksDoc);
        }
    }

}
