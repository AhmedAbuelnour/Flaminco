using System.Text.Json.Serialization;

namespace Flaminco.Keycloak.Models
{
    public class Groups
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("path")] public string Path { get; set; }
    }
}
