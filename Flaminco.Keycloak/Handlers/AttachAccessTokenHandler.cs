using Flaminco.Keycloak.Exceptions;
using Flaminco.Keycloak.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Flaminco.Keycloak.Handlers
{
    public class AttachAccessTokenHandler(IHttpClientFactory _httpClientFactory, IOptions<KeycloakOptions> _keycloakOptions) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage tokenResponse = await _httpClientFactory.CreateClient("AccessKeycloakClient").PostAsync($"realms/{_keycloakOptions.Value.Realm}/protocol/openid-connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", _keycloakOptions.Value.Credentials.ClientId },
                { "client_secret", _keycloakOptions.Value.Credentials.ClientSecret }
            }), cancellationToken);

            tokenResponse.EnsureSuccessStatusCode();

            if (await tokenResponse.Content.ReadFromJsonAsync<Token>(cancellationToken) is Token token)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            }
            else
            {
                throw new AccessTokenGenerateException();
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}