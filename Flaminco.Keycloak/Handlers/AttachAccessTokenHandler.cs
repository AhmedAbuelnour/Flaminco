using Flaminco.Keycloak.Constants;
using Flaminco.Keycloak.Exceptions;
using Flaminco.Keycloak.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Flaminco.Keycloak.Handlers
{
    internal sealed class AttachAccessTokenHandler(IHttpClientFactory _httpClientFactory, IOptions<KeycloakOptions> _keycloakOptions, IMemoryCache _memoryCache) : DelegatingHandler
    {
        private readonly string _cacheKey = $"access_token_{_keycloakOptions.Value.Credentials.ClientId}";
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!_memoryCache.TryGetValue(_cacheKey, out KeycloakToken? token))
            {
                HttpResponseMessage tokenResponse = await _httpClientFactory.CreateClient(Constant.KeycloakAccessTokenClient).PostAsync($"realms/{_keycloakOptions.Value.Realm}/protocol/openid-connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", _keycloakOptions.Value.Credentials.ClientId },
                    { "client_secret", _keycloakOptions.Value.Credentials.ClientSecret }
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