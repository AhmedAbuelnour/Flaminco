namespace Flaminco.Keycloak.Client.Exceptions
{
    internal sealed class GroupNotFoundException(string groupName) : Exception($"Group Name: {groupName} not found")
    {
    }
}
