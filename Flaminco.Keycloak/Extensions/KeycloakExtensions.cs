using Flaminco.Keycloak.Authentication.JwtBearer.ClaimsTransformations;
using Flaminco.Keycloak.Authentication.JwtBearer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Flaminco.Keycloak.Authentication.JwtBearer.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring Keycloak authentication and client services.
    /// </summary>
    public static class KeycloakExtensions
    {
        /// <summary>
        /// Adds Keycloak JWT Bearer authentication to the service collection using the specified options action.
        /// </summary>
        /// <param name="services">The service collection to add the authentication to.</param>
        /// <param name="configureOptions">An action to configure the Keycloak options.</param>
        /// <returns>The service collection with Keycloak authentication added.</returns>
        public static IServiceCollection AddKeycloakJwtBearerAuthentication(this IServiceCollection services, Action<KeycloakOptions> configureOptions)
        {
            KeycloakOptions keycloakOptions = new();

            configureOptions(keycloakOptions);

            services.Configure(configureOptions);

            keycloakOptions.Validate();

            return AddKeycloakJwtBearerAuthenticationInternal(services, keycloakOptions);
        }

        /// <summary>
        /// Internal method to add Keycloak JWT Bearer authentication using the specified Keycloak options.
        /// </summary>
        /// <param name="services">The service collection to add the authentication to.</param>
        /// <param name="keycloakOptions">The configured Keycloak options.</param>
        /// <returns>The service collection with Keycloak authentication added.</returns>
        private static IServiceCollection AddKeycloakJwtBearerAuthenticationInternal(IServiceCollection services, KeycloakOptions keycloakOptions)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = keycloakOptions.Authority;
                options.SaveToken = keycloakOptions.SaveToken;
                options.RequireHttpsMetadata = keycloakOptions.RequireHttpsMetadata;
                options.MetadataAddress = keycloakOptions.MetadataAddress;
                options.AutomaticRefreshInterval = keycloakOptions.JwksOption.AutomaticRefreshInterval ?? BaseConfigurationManager.DefaultAutomaticRefreshInterval;
                options.RefreshInterval = keycloakOptions.JwksOption.RefreshInterval ?? BaseConfigurationManager.DefaultRefreshInterval;
                options.BackchannelHttpHandler = keycloakOptions.JwksOption.BackchannelHttpHandler;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidAudience = keycloakOptions.Audience,
                    ValidIssuer = keycloakOptions.Issuer,
                    RoleClaimType = keycloakOptions.RoleClaimType,
                    NameClaimType = keycloakOptions.NameClaimType,
                    ClockSkew = keycloakOptions.ClockSkew ?? TimeSpan.FromSeconds(15),
                };

                options.SetJwksOptions(keycloakOptions.CertsAddress);

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
    }
}
