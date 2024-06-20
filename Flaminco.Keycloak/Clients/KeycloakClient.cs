using Flaminco.Keycloak.Constants;
using Flaminco.Keycloak.Exceptions;
using Flaminco.Keycloak.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Flaminco.Keycloak.Clients
{
    /// <summary>
    /// Implementation of the Keycloak client for interacting with the Keycloak server.
    /// </summary>
    public sealed class KeycloakClient(IHttpClientFactory httpClientFactory, ILogger<KeycloakClient> logger) : IKeycloakClient
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient(Constant.KeycloakClient);

        /// <summary>
        /// Adds a specified user to a specified group in Keycloak.
        /// </summary>
        /// <param name="userId">The ID of the user to add to the group.</param>
        /// <param name="groupName">The name of the group to add the user to.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Removes a specified user from a specified group in Keycloak.
        /// </summary>
        /// <param name="userId">The ID of the user to remove from the group.</param>
        /// <param name="groupName">The name of the group to remove the user from.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Retrieves all groups from Keycloak.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, containing a collection of Keycloak groups.</returns>
        public async Task<IEnumerable<KeycloakGroup>> GetGroupsAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Retrieving groups");

            HttpResponseMessage response = await _httpClient.GetAsync($"groups", cancellationToken);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<IEnumerable<KeycloakGroup>>(cancellationToken) ?? [];
        }

        /// <summary>
        /// Disables a specified user in Keycloak.
        /// </summary>
        /// <param name="userId">The ID of the user to disable.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Enables a specified user in Keycloak.
        /// </summary>
        /// <param name="userId">The ID of the user to enable.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Deletes a specified user from Keycloak.
        /// </summary>
        /// <param name="userId">The ID of the user to delete.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Deleting user {UserId}", userId);

            HttpResponseMessage response = await _httpClient.DeleteAsync($"users/{userId}", cancellationToken);

            response.EnsureSuccessStatusCode();

            logger.LogInformation("User {UserId} deleted", userId);
        }

        /// <summary>
        /// Retrieves account information for a specified user from Keycloak.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve information for.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, containing the user's account information, or null if the user is not found.</returns>
        public async Task<KeycloakUser?> GetUserAccountInfoAsync(string userId, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Retrieving user account info for {UserId}", userId);

            HttpResponseMessage response = await _httpClient.GetAsync($"users/{userId}", cancellationToken);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<KeycloakUser>(cancellationToken);
        }

        /// <summary>
        /// Retrieves all users from a specified group in Keycloak.
        /// </summary>
        /// <param name="groupName">The name of the group to retrieve users from.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, containing a collection of users in the group.</returns>
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

        /// <summary>
        /// Retrieves all users from Keycloak.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, containing a collection of Keycloak users.</returns>
        public async Task<IEnumerable<KeycloakUser>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Retrieving all users");

            HttpResponseMessage response = await _httpClient.GetAsync($"users", cancellationToken);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<IEnumerable<KeycloakUser>>(cancellationToken) ?? [];
        }

        /// <summary>
        /// Adds attributes to a specified user in Keycloak.
        /// </summary>
        /// <param name="userId">The ID of the user to add attributes to.</param>
        /// <param name="attributes">A dictionary of attributes to add, where the key is the attribute name and the value is a collection of attribute values.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Updates the username of a specified user in Keycloak.
        /// </summary>
        /// <param name="userId">The ID of the user to update.</param>
        /// <param name="userName">The new username of the user.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Updates the email of a specified user in Keycloak.
        /// </summary>
        /// <param name="userId">The ID of the user to update.</param>
        /// <param name="email">The new email address of the user.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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