
namespace Flaminco.Keycloak.Models
{
    public class KeycloakOptions
    {
        public string AuthUrl { get; set; }
        public string Realm { get; set; }
        public string Audience { get; set; }
        public bool RequireHttpsMetadata { get; set; }
        public TimeSpan? ClockSkew { get; set; }
        public string RoleClaimType { get; set; } = "role";
        public string NameClaimType { get; set; } = "name";
        public bool SaveToken { get; set; }
        public KeycloakCredentials Credentials { get; set; }
    }

    public class KeycloakCredentials
    {
        public string ClientSecret { get; set; }
        public string ClientId { get; set; }
    }
}
