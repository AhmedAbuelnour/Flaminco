using Flaminco.Keycloak.Constants;
using Flaminco.Keycloak.Exceptions;
using Flaminco.Keycloak.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Flaminco.Keycloak.Clients
{
    public sealed class KeycloakClient(IHttpClientFactory httpClientFactory, ILogger<KeycloakClient> logger) : IKeycloakClient
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient(Constant.KeycloakClient);

        public async Task AddUserToGroupAsync(string userId, string groupName, CancellationToken cancellationToken = default)
        {
            IEnumerable<Groups> groups = await GetGroupsAsync(cancellationToken);

            if (groups.FirstOrDefault(a => a.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase)) is Groups foundGroup)
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

        public async Task RemoveUserFromGroupAsync(string userId, string groupName, CancellationToken cancellationToken = default)
        {
            IEnumerable<Groups> groups = await GetGroupsAsync(cancellationToken);

            if (groups.FirstOrDefault(a => a.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase)) is Groups foundGroup)
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

        public async Task<IEnumerable<Groups>> GetGroupsAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Retrieving groups");

            HttpResponseMessage response = await _httpClient.GetAsync($"groups", cancellationToken);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<IEnumerable<Groups>>(cancellationToken);
        }

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

        public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Deleting user {UserId}", userId);

            HttpResponseMessage response = await _httpClient.DeleteAsync($"users/{userId}", cancellationToken);

            response.EnsureSuccessStatusCode();

            logger.LogInformation("User {UserId} deleted", userId);
        }

        public async Task<UserAccount> GetUserAccountInfoAsync(string userId, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Retrieving user account info for {UserId}", userId);

            HttpResponseMessage response = await _httpClient.GetAsync($"users/{userId}", cancellationToken);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<UserAccount>(cancellationToken);
        }

        public async Task<IEnumerable<UserAccount>> GetUsersFromGroupAsync(string groupName, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Retrieving users from group {GroupName}", groupName);

            IEnumerable<Groups> groups = await GetGroupsAsync(cancellationToken);

            if (groups.FirstOrDefault(a => a.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase)) is Groups foundGroup)
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"groups/{foundGroup.Id}/members", cancellationToken);

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<UserAccount>>(cancellationToken) ?? [];
            }
            else
            {
                throw new GroupNotFoundException(groupName);
            }

        }

        public async Task<IEnumerable<UserAccount>> GetAllUsersAsync(int first, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Retrieving all users starting from {First}", first);

            HttpResponseMessage response = await _httpClient.GetAsync($"users?first={first}", cancellationToken);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<IEnumerable<UserAccount>>(cancellationToken) ?? [];
        }

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
    }
}