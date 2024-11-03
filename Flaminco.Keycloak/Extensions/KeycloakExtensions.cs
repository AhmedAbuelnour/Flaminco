﻿using System.Net.Http.Headers;
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

namespace Flaminco.Keycloak.Extensions;

/// <summary>
///     Provides extension methods for configuring Keycloak authentication and client services.
/// </summary>
public static class KeycloakExtensions
{
    /// <summary>
    ///     Adds Keycloak JWT Bearer authentication to the service collection using the specified configuration section.
    /// </summary>
    /// <param name="services">The service collection to add the authentication to.</param>
    /// <param name="configuration">The configuration to bind the Keycloak options from.</param>
    /// <param name="sectionName">The configuration section name containing Keycloak settings. Default is "Keycloak".</param>
    /// <returns>The service collection with Keycloak authentication added.</returns>
    public static IServiceCollection AddKeycloakJwtBearerAuthentication(this IServiceCollection services,
        IConfiguration configuration, string sectionName = "Keycloak")
    {
        KeycloakOptions keycloakOptions = new();

        configuration.GetSection(sectionName).Bind(keycloakOptions);

        services.Configure<KeycloakOptions>(configuration.GetSection(sectionName));

        keycloakOptions.Validate();

        return AddKeycloakJwtBearerAuthenticationInternal(services, keycloakOptions);
    }

    /// <summary>
    ///     Adds Keycloak JWT Bearer authentication to the service collection using the specified options action.
    /// </summary>
    /// <param name="services">The service collection to add the authentication to.</param>
    /// <param name="configureOptions">An action to configure the Keycloak options.</param>
    /// <returns>The service collection with Keycloak authentication added.</returns>
    public static IServiceCollection AddKeycloakJwtBearerAuthentication(this IServiceCollection services,
        Action<KeycloakOptions> configureOptions)
    {
        KeycloakOptions keycloakOptions = new();

        configureOptions(keycloakOptions);

        services.Configure(configureOptions);

        keycloakOptions.Validate();

        return AddKeycloakJwtBearerAuthenticationInternal(services, keycloakOptions);
    }

    /// <summary>
    ///     Internal method to add Keycloak JWT Bearer authentication using the specified Keycloak options.
    /// </summary>
    /// <param name="services">The service collection to add the authentication to.</param>
    /// <param name="keycloakOptions">The configured Keycloak options.</param>
    /// <returns>The service collection with Keycloak authentication added.</returns>
    private static IServiceCollection AddKeycloakJwtBearerAuthenticationInternal(IServiceCollection services,
        KeycloakOptions keycloakOptions)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.Authority = $"{keycloakOptions.AuthUrl}/realms/{keycloakOptions.Realm}";
            options.SaveToken = keycloakOptions.SaveToken;
            options.RequireHttpsMetadata = keycloakOptions.RequireHttpsMetadata;
            options.MetadataAddress =
                $"{keycloakOptions.AuthUrl}/realms/{keycloakOptions.Realm}/.well-known/openid-configuration";

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidAudience = keycloakOptions.Audience,
                ValidIssuer = keycloakOptions.Issuer,
                RoleClaimType = keycloakOptions.RoleClaimType,
                NameClaimType = keycloakOptions.NameClaimType,
                ClockSkew = keycloakOptions.ClockSkew ?? TimeSpan.FromSeconds(15)
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (!string.IsNullOrEmpty(context.Request.Query["access_token"]))
                        // Read the token out of the query string
                        context.Token = context.Request.Query["access_token"];
                    return Task.CompletedTask;
                },

                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        context.Response.Headers["Token-Expired"] = "true";

                    if (context.Exception.GetType() == typeof(SecurityTokenValidationException))
                        context.Response.Headers["Token-Validation"] = "false";

                    return Task.CompletedTask;
                }
            };
        });

        services.AddScoped<IClaimsTransformation>(_ =>
            new KeycloakRolesClaimsTransformation(keycloakOptions.RoleClaimType, keycloakOptions.Audience));

        return services;
    }

    /// <summary>
    ///     Adds Keycloak client services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the Keycloak client services to.</param>
    /// <returns>The service collection with Keycloak client services added.</returns>
    public static IServiceCollection AddKeycloakClient(this IServiceCollection services)
    {
        services.AddMemoryCache();

        services.AddTransient<AttachAccessTokenHandler>();

        services.AddHttpClient(Constant.KeycloakAccessTokenClient, (serviceProvider, client) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<KeycloakOptions>>().Value;

            client.BaseAddress = new Uri(settings.AuthUrl);

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        services.AddHttpClient(Constant.KeycloakClient, (serviceProvider, client) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<KeycloakOptions>>().Value;

            client.BaseAddress = new Uri($"{settings.AuthUrl}/admin/realms/{settings.Realm}/");

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }).AddHttpMessageHandler<AttachAccessTokenHandler>();

        services.AddScoped<IKeycloakClient, KeycloakClient>();

        return services;
    }
}