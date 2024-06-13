using Flaminco.Keycloak.Models;

namespace Flaminco.Keycloak.Clients
{
    public interface IKeycloakClient
    {
        Task AddAttributesToUserAsync(string userId, Dictionary<string, IEnumerable<object>> attributes, CancellationToken cancellationToken = default);
        Task AddUserToGroupAsync(string userId, string groupName, CancellationToken cancellationToken = default);
        Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
        Task DisableUserAsync(string userId, CancellationToken cancellationToken = default);
        Task EnableUserAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserAccount>> GetAllUsersAsync(int first, CancellationToken cancellationToken = default);
        Task<IEnumerable<Groups>> GetGroupsAsync(CancellationToken cancellationToken = default);
        Task<UserAccount> GetUserAccountInfoAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserAccount>> GetUsersFromGroupAsync(string groupName, CancellationToken cancellationToken = default);
        Task RemoveUserFromGroupAsync(string userId, string groupName, CancellationToken cancellationToken = default);
        Task UpdateEmailAsync(string userId, string email, CancellationToken cancellationToken = default);
        Task UpdateUsernameAsync(string userId, string userName, CancellationToken cancellationToken = default);
    }
}
