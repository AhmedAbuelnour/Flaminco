using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Flaminco.JWTHandler.JWTModel;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Flaminco.JWTHandler.TokenHandler;

public class TokenHandlerManager
{
    private readonly JWTConfigurationOptions _configuration;

    public TokenHandlerManager(IOptions<JWTConfigurationOptions> options)
    {
        _configuration = options.Value;
    }

    public AccessToken GetAccessToken(Dictionary<string, string> userProfileClaims)
    {
        JwtSecurityToken token = new(
            _configuration.Issuer,
            _configuration.Audience,
            notBefore: DateTime.UtcNow,
            claims: userProfileClaims.Select(claim => new Claim(claim.Key, claim.Value))
                .Union([new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())]),
            expires: DateTime.UtcNow.Add(_configuration.AccessTokenExpiration),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Convert.FromBase64String(_configuration.Key)),
                SecurityAlgorithms.HmacSha256Signature));
        if (_configuration.ClearCliamTypeMap)
        {
            // To stop mapping the Claim type long schema name to short ones.
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();
        }

        return new AccessToken
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            CreatedOn = DateTime.UtcNow,
            ExpiresOn = DateTime.UtcNow.Add(_configuration.AccessTokenExpiration)
        };
    }

    public RefreshToken GetRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return new RefreshToken
        {
            Token = Convert.ToBase64String(randomNumber),
            CreatedOn = DateTime.UtcNow,
            ExpiresOn = DateTime.UtcNow.Add(_configuration.RefreshTokenExpiration)
        };
    }

    public AccessToken GetRefreshAccessToken(string expiredToken)
    {
        return GetAccessToken(GetPrincipalFromExpiredToken(expiredToken));
    }

    private Dictionary<string, string> GetPrincipalFromExpiredToken(string expiredToken)
    {
        TokenValidationParameters tokenValidationParameters = new()
        {
            ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(_configuration.Key)),
            ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(expiredToken, tokenValidationParameters, out var securityToken);
        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature,
                StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");
        return principal.Claims.ToDictionary(m => m.Type, m => m.Value);
    }
}