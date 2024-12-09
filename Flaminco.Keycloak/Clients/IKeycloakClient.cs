using Flaminco.Keycloak.Models;

namespace Flaminco.Keycloak.Clients
{
    /// <summary>
    /// Interface for Keycloak client operations.
    /// </summary>
    public interface IKeycloakClient
    {

        /// <summary>
        /// Adds attributes to a specified user in Keycloak.
        /// </summary>
        /// <param name="userId">The ID of the user to add attributes to.</param>
        /// <param name="attributes">A dictionary of attributes to add, where the key is the attribute name and the value is a collection of attribute values.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddAttributesToUserAsync(string userId, Dictionary<string, IEnumerable<object>> attributes, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a specified user to a specified group in Keycloak.
        /// </summary>
        /// <param name="userId">The ID of the user to add to the group.</param>
        /// <param name="groupName">The name of the group to add the user to.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddUserToGroupAsync(string userId, string groupName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a specified user from Keycloak.
        /// </summary>
        /// <param name="userId">The ID of the user to delete.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Disables a specified user in Keycloak.
        /// </summary>
        /// <param name="userId">The ID of the user to disable.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DisableUserAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Enables a specified user in Keycloak.
        /// </summary>
        /// <param name="userId">The ID of the user to enable.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task EnableUserAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all users from Keycloak.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, containing a collection of Keycloak users.</returns>
        Task<IEnumerable<KeycloakUser>> GetAllUsersAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all groups from Keycloak.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, containing a collection of Keycloak groups.</returns>
        Task<IEnumerable<KeycloakGroup>> GetGroupsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves account information for a specified user from Keycloak.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve information for.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, containing the user's account information, or null if the user is not found.</returns>
        Task<KeycloakUser?> GetUserAccountInfoAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a user by a specified attribute and value from Keycloak.
        /// </summary>
        /// <param name="attributeName">The name of the attribute to search for.</param>
        /// <param name="attributeValue">The value of the attribute to search for.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, containing the user matching the attribute, or null if not found.</returns>
        Task<KeycloakUser?> GetUserByAttributeAsync(string attributeName, string attributeValue, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a user by their email address from Keycloak.
        /// </summary>
        /// <param name="email">The email address of the user to retrieve.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, containing the user with the specified email, or null if not found.</returns>
        Task<KeycloakUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);


        /// <summary>
        /// Retrieves users by a specified attribute and value from Keycloak.
        /// </summary>
        /// <param name="attributeName">The name of the attribute to search for.</param>
        /// <param name="attributeValue">The value of the attribute to search for.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, containing a collection of users matching the attribute.</returns>
        Task<IEnumerable<KeycloakUser>> GetUsersByAttributeAsync(string attributeName, string attributeValue, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all users from a specified group in Keycloak.
        /// </summary>
        /// <param name="groupName">The name of the group to retrieve users from.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, containing a collection of users in the group.</returns>
        Task<IEnumerable<KeycloakUser>> GetUsersFromGroupAsync(string groupName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a specified user from a specified group in Keycloak.
        /// </summary>
        /// <param name="userId">The ID of the user to remove from the group.</param>
        /// <param name="groupName">The name of the group to remove the user from.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveUserFromGroupAsync(string userId, string groupName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the email of a specified user in Keycloak.
        /// </summary>
        /// <param name="userId">The ID of the user to update.</param>
        /// <param name="email">The new email address of the user.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateEmailAsync(string userId, string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the username of a specified user in Keycloak.
        /// </summary>
        /// <param name="userId">The ID of the user to update.</param>
        /// <param name="userName">The new username of the user.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateUsernameAsync(string userId, string userName, CancellationToken cancellationToken = default);
    }
}
