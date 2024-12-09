namespace Flaminco.Keycloak.Exceptions
{
    internal sealed class GroupNotFoundException(string groupName) : Exception($"Group Name: {groupName} not found")
    {
    }
}
