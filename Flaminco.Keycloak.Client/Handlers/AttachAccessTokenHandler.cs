using Flaminco.Keycloak.Authentication.Models;
using Flaminco.Keycloak.Client.Constants;
using Flaminco.Keycloak.Client.Exceptions;
using Flaminco.Keycloak.Client.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Flaminco.Keycloak.Client.Handlers
{
    internal sealed class AttachAccessTokenHandler(IHttpClientFactory _httpClientFactory, IOptions<KeycloakClientOptions> _keycloakClientOptions, IMemoryCache _memoryCache) : DelegatingHandler
    {
        private readonly string _cacheKey = $"access_token_{_keycloakClientOptions.Value.Credentials.ClientId}";
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!_memoryCache.TryGetValue(_cacheKey, out KeycloakToken? token))
            {
                HttpResponseMessage tokenResponse = await _httpClientFactory.CreateClient(Constant.KeycloakAccessTokenClient).PostAsync($"realms/{_keycloakClientOptions.Value.Realm}/protocol/openid-connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", _keycloakClientOptions.Value.Credentials.ClientId },
                    { "client_secret", _keycloakClientOptions.Value.Credentials.ClientSecret }
                }), cancellationToken);

                tokenResponse.EnsureSuccessStatusCode();

                token = await tokenResponse.Content.ReadFromJsonAsync<KeycloakToken>(cancellationToken);

                if (token is null)
                {
                    throw new AccessTokenGenerateException();
                }

                _memoryCache.Set(_cacheKey, token, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(token.ExpiresIn - 30)));
            }

            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token!.AccessToken);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}