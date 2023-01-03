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

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)

       .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
       {
           options.RequireHttpsMetadata = jwtOptions.RequireHttpsMetadata;
           options.SaveToken = jwtOptions.SaveTokenInAuthProperties;
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuerSigningKey = jwtOptions.ValidateIssuerSigningKey,
               ValidateLifetime = jwtOptions.ValidateLifetime,
               ValidateIssuer = jwtOptions.ValidateIssuer,
               ValidateAudience = jwtOptions.ValidateAudience,
               ValidAudience = jwtOptions.Audience,
               ValidIssuer = jwtOptions.Issuer,
               IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtOptions.Key)),
               ClockSkew = jwtOptions.ClockSkew
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


        services.AddAuthorization();

        services.AddSingleton<TokenHandlerManager>();

        return services;
    }
}

