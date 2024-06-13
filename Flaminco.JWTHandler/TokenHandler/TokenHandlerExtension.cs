using Flaminco.JWTHandler.JWTModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Flaminco.JWTHandler.TokenHandler;

public static class TokenHandlerExtension
{
    public static IServiceCollection AddJwtBearer(this IServiceCollection services, IConfiguration configuration)
    {
        JWTConfigurationOptions jwtOptions = new();

        IConfigurationSection configurationSection = configuration.GetSection(nameof(JWTConfigurationOptions));

        configurationSection.Bind(jwtOptions);

        services.Configure<JWTConfigurationOptions>(configurationSection);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
       {
           options.RequireHttpsMetadata = jwtOptions.RequireHttpsMetadata;
           options.SaveToken = jwtOptions.SaveTokenInAuthProperties;
           options.Audience = jwtOptions.Audience;
           options.Authority = jwtOptions.Authority;
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuerSigningKey = jwtOptions.ValidateIssuerSigningKey,
               ValidateLifetime = jwtOptions.ValidateLifetime,
               ValidateIssuer = jwtOptions.ValidateIssuer,
               ValidateAudience = jwtOptions.ValidateAudience,
               ValidAudience = jwtOptions.Audience,
               ValidIssuer = jwtOptions.Issuer,
               IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtOptions.Key)),
               ClockSkew = jwtOptions.ClockSkew,
               RoleClaimType = jwtOptions.RoleClaimType,

           };
           options.Events = new JwtBearerEvents
           {
               OnMessageReceived = context =>
               {
                   if (!string.IsNullOrEmpty(context.Request.Query["access_token"]))
                   {
                       // Read the token out of the query string
                       context.Token = context.Request.Query["access_token"];
                   }
                   return Task.CompletedTask;
               },
               OnAuthenticationFailed = context =>
               {
                   if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                   {
                       context.Response.Headers["Token-Expired"] = "true";
                   }

                   if (context.Exception.GetType() == typeof(SecurityTokenValidationException))
                   {
                       context.Response.Headers["Token-Validation"] = "false";
                   }

                   return Task.CompletedTask;
               }
           };
       });


        services.AddAuthorization();

        services.AddSingleton<TokenHandlerManager>();

        return services;
    }
}

