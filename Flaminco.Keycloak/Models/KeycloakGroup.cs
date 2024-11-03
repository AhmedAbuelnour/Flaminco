using System.Text.Json.Serialization;

namespace Flaminco.Keycloak.Models;

/// <summary>
///     Represents a group in Keycloak.
/// </summary>
public class KeycloakGroup
{
    /// <summary>
    ///     Gets or sets the ID of the group.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    ///     Gets or sets the name of the group.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    ///     Gets or sets the path of the group.
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; set; }
}