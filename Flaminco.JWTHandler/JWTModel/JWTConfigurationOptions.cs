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
    public bool ValidateIssuerSigningKey { get; set; } = true;
    public bool ValidateLifetime { get; set; } = true;
    public bool ValidateIssuer { get; set; } = true;
    public bool ValidateAudience { get; set; } = true;
    public TimeSpan ClockSkew { get; set; } = TimeSpan.Zero;
    public string? Authority { get; set; }
    public string? RoleClaimType { get; set; }
}