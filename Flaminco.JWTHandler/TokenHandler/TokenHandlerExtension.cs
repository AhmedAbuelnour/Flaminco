using Flaminco.JWTHandler.JWTModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Flaminco.JWTHandler.TokenHandler;

public static class TokenHandlerExtension
{
    public static IServiceCollection AddJWTTokenHandlerExtension(this IServiceCollection services, Action<JWTConfigurationOptions> jwtOptions)
    {
        services.Configure(jwtOptions);

        JWTConfigurationOptions configuration = new();

        jwtOptions.Invoke(configuration);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = configuration.RequireHttpsMetadata;
            options.SaveToken = configuration.SaveTokenInAuthProperties;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = configuration.Audience,
                ValidIssuer = configuration.Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(configuration.Key)),
                ClockSkew = TimeSpan.Zero // once is expired, it is not valid.
            };
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.Add("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                }
            };
        });

        services.AddSingleton<TokenHandlerManager>();

        return services;
    }
}

