namespace Flaminco.Keycloak.Exceptions
{
    public class GroupNotFoundException(string groupName) : Exception($"Group Name: {groupName} not found")
    {
    }
}
