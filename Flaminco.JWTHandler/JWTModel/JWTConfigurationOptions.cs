namespace Flaminco.JWTHandler.JWTModel;
public class JWTConfigurationOptions
{
    public string Key { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public TimeSpan AccessTokenExpiration { get; set; } = TimeSpan.FromDays(14);
    public TimeSpan RefreshTokenExpiration { get; set; } = TimeSpan.FromDays(90);
    public bool ClearCliamTypeMap { get; set; } = false;
    public bool RequireHttpsMetadata { get; set; } = false;
    public bool SaveTokenInAuthProperties { get; set; } = false;
}

