using Flaminco.Keycloak.Client.Constants;
using Flaminco.Keycloak.Client.Exceptions;
using Flaminco.Keycloak.Client.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Flaminco.Keycloak.Client.Clients
{
    /// <summary>
    /// Implementation of the Keycloak client for interacting with the Keycloak server.
    /// </summary>
    public sealed class KeycloakClient(IHttpClientFactory httpClientFactory, ILogger<KeycloakClient> logger) : IKeycloakClient
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient(Constant.KeycloakClient);

        /// <inheritdoc/>
        public async Task AddUserToGroupAsync(string userId, string groupName, CancellationToken cancellationToken = default)
        {
            IEnumerable<KeycloakGroup> groups = await GetGroupsAsync(cancellationToken);

            if (groups.FirstOrDefault(a => a.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase)) is KeycloakGroup foundGroup)
            {
                logger.LogInformation("Adding user {UserId} to group {GroupId}", userId, foundGroup.Id);

                HttpResponseMessage response = await _httpClient.PutAsync($"users/{userId}/groups/{foundGroup.Id}", null, cancellationToken);

                response.EnsureSuccessStatusCode();

                logger.LogInformation("User {UserId} added to group {GroupId}", userId, foundGroup.Id);
            }
            else
            {
                throw new GroupNotFoundException(groupName);
            }
        }

        /// <inheritdoc/>
        public async Task RemoveUserFromGroupAsync(string userId, string groupName, CancellationToken cancellationToken = default)
        {
            IEnumerable<KeycloakGroup> groups = await GetGroupsAsync(cancellationToken);

            if (groups.FirstOrDefault(a => a.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase)) is KeycloakGroup foundGroup)
            {
                logger.LogInformation("Removing user {UserId} from group {GroupId}", userId, foundGroup.Id);

                HttpResponseMessage response = await _httpClient.DeleteAsync($"users/{userId}/groups/{foundGroup.Id}", cancellationToken);

                response.EnsureSuccessStatusCode();

                logger.LogInformation("User {UserId} removed from group {GroupId}", userId, foundGroup.Id);
            }
            else
            {
                throw new GroupNotFoundException(groupName);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<KeycloakGroup>> GetGroupsAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Retrieving groups");

            HttpResponseMessage response = await _httpClient.GetAsync($"groups", cancellationToken);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<IEnumerable<KeycloakGroup>>(cancellationToken) ?? [];
        }

        /// <inheritdoc/>
        public async Task DisableUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Disabling user {UserId}", userId);

            HttpResponseMessage response = await _httpClient.PutAsync($"users/{userId}", new StringContent(JsonSerializer.Serialize(new
            {
                enabled = false
            }), Encoding.UTF8, "application/json"), cancellationToken);

            response.EnsureSuccessStatusCode();

            logger.LogInformation("User {UserId} disabled", userId);
        }

        /// <inheritdoc/>
        public async Task EnableUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Enabling user {UserId}", userId);

            HttpResponseMessage response = await _httpClient.PutAsync($"users/{userId}", new StringContent(JsonSerializer.Serialize(new
            {
                enabled = true
            }), Encoding.UTF8, "application/json"), cancellationToken);

            response.EnsureSuccessStatusCode();

            logger.LogInformation("User {UserId} enabled", userId);
        }

        /// <inheritdoc/>
        public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Deleting user {UserId}", userId);

            HttpResponseMessage response = await _httpClient.DeleteAsync($"users/{userId}", cancellationToken);

            response.EnsureSuccessStatusCode();

            logger.LogInformation("User {UserId} deleted", userId);
        }

        /// <inheritdoc/>
        public async Task<KeycloakUser?> GetUserAccountInfoAsync(string userId, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Retrieving user account info for {UserId}", userId);

            HttpResponseMessage response = await _httpClient.GetAsync($"users/{userId}", cancellationToken);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<KeycloakUser>(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<KeycloakUser>> GetUsersFromGroupAsync(string groupName, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Retrieving users from group {GroupName}", groupName);

            IEnumerable<KeycloakGroup> groups = await GetGroupsAsync(cancellationToken);

            if (groups.FirstOrDefault(a => a.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase)) is KeycloakGroup foundGroup)
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"groups/{foundGroup.Id}/members", cancellationToken);

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<KeycloakUser>>(cancellationToken) ?? [];
            }
            else
            {
                throw new GroupNotFoundException(groupName);
            }

        }

        /// <inheritdoc/>
        public async Task<IEnumerable<KeycloakUser>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Retrieving all users");

            HttpResponseMessage response = await _httpClient.GetAsync($"users", cancellationToken);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<IEnumerable<KeycloakUser>>(cancellationToken) ?? [];
        }

        /// <inheritdoc/>
        public async Task AddAttributesToUserAsync(string userId, Dictionary<string, IEnumerable<object>> attributes, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Adding attributes to user {UserId}", userId);

            HttpResponseMessage response = await _httpClient.PutAsync($"users/{userId}", new StringContent(JsonSerializer.Serialize(new
            {
                attributes
            }), Encoding.UTF8, "application/json"), cancellationToken);

            response.EnsureSuccessStatusCode();

            logger.LogInformation("Attributes added to user {UserId}", userId);
        }

        /// <inheritdoc/>
        public async Task UpdateUsernameAsync(string userId, string userName, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Updating username for user {UserId}", userId);

            HttpResponseMessage response = await _httpClient.PutAsync($"users/{userId}", new StringContent(JsonSerializer.Serialize(new
            {
                username = userName,
            }), Encoding.UTF8, "application/json"), cancellationToken);


            response.EnsureSuccessStatusCode();

            logger.LogInformation("Username updated for user {UserId}", userId);
        }

        /// <inheritdoc/>
        public async Task UpdateEmailAsync(string userId, string email, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Updating username for user {UserId}", userId);

            HttpResponseMessage response = await _httpClient.PutAsync($"users/{userId}", new StringContent(JsonSerializer.Serialize(new
            {
                email,
            }), Encoding.UTF8, "application/json"), cancellationToken);


            response.EnsureSuccessStatusCode();

            logger.LogInformation("Username updated for user {UserId}", userId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<KeycloakUser>> GetUsersByAttributeAsync(string attributeName, string attributeValue, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Get Users by attribute name {name}, and attribute value {value} ", attributeName, attributeValue);

            HttpResponseMessage response = await _httpClient.GetAsync($"users?attributes.{attributeName}={attributeValue}", cancellationToken);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<IEnumerable<KeycloakUser>>(cancellationToken) ?? [];
        }

        /// <inheritdoc/>
        public async Task<KeycloakUser?> GetUserByAttributeAsync(string attributeName, string attributeValue, CancellationToken cancellationToken = default)
        {
            return (await GetUsersByAttributeAsync(attributeName, attributeValue, cancellationToken)).FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<KeycloakUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Get User by email {email}", email);

            HttpResponseMessage response = await _httpClient.GetAsync($"users?email={email}", cancellationToken);

            response.EnsureSuccessStatusCode();

            return (await response.Content.ReadFromJsonAsync<IEnumerable<KeycloakUser>>(cancellationToken) ?? []).FirstOrDefault();
        }

    }
}