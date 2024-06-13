using Flaminco.Keycloak.ClaimsTransformations;
using Flaminco.Keycloak.Clients;
using Flaminco.Keycloak.Constants;
using Flaminco.Keycloak.Handlers;
using Flaminco.Keycloak.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;

namespace Flaminco.Keycloak.Extensions
{
    public static class KeycloakExtensions
    {
        public static IServiceCollection AddKeycloakJwtBearerAuthentication(this IServiceCollection services, IConfiguration configuration, string sectionName = "Keycloak")
        {
            KeycloakOptions keycloakOptions = new();

            configuration.GetSection(sectionName).Bind(keycloakOptions);

            services.Configure<KeycloakOptions>(configuration.GetSection(sectionName));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
             {
                 options.Authority = $"{keycloakOptions.AuthUrl}/realms/{keycloakOptions.Realm}";
                 options.SaveToken = keycloakOptions.SaveToken;
                 options.RequireHttpsMetadata = keycloakOptions.RequireHttpsMetadata;
                 options.MetadataAddress = $"{keycloakOptions.AuthUrl}/realms/{keycloakOptions.Realm}/.well-known/openid-configuration";

                 options.TokenValidationParameters = new TokenValidationParameters()
                 {
                     ValidateIssuerSigningKey = true,
                     ValidateIssuer = true,
                     ValidateAudience = true,
                     ValidAudience = keycloakOptions.Audience,
                     RoleClaimType = keycloakOptions.RoleClaimType,
                     NameClaimType = keycloakOptions.NameClaimType,
                     ClockSkew = keycloakOptions.ClockSkew ?? TimeSpan.FromSeconds(15),
                 };

                 options.Events = new JwtBearerEvents()
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


            services.AddScoped<IClaimsTransformation>(_ => new KeycloakRolesClaimsTransformation(keycloakOptions.RoleClaimType, keycloakOptions.Audience));

            return services;
        }

        public static IServiceCollection AddKeycloakService(this IServiceCollection services)
        {
            services.AddTransient<AttachAccessTokenHandler>();

            services.AddHttpClient(Constant.KeycloakAccessTokenClient, (serviceProvider, client) =>
            {
                KeycloakOptions settings = serviceProvider.GetRequiredService<IOptions<KeycloakOptions>>().Value;

                client.BaseAddress = new Uri(settings.AuthUrl);

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

            services.AddHttpClient(Constant.KeycloakClient, (serviceProvider, client) =>
            {
                KeycloakOptions settings = serviceProvider.GetRequiredService<IOptions<KeycloakOptions>>().Value;

                client.BaseAddress = new Uri($"{settings.AuthUrl}/admin/realms/{settings.Realm}/");

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            }).AddHttpMessageHandler<AttachAccessTokenHandler>();

            services.AddScoped<IKeycloakClient, KeycloakClient>();

            return services;
        }
    }
}
