using Flaminco.Keycloak.Authentication.Models;
using Flaminco.Keycloak.Client.Clients;
using Flaminco.Keycloak.Client.Constants;
using Flaminco.Keycloak.Client.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace Flaminco.Keycloak.Client.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring Keycloak authentication and client services.
    /// </summary>
    public static class KeycloakExtensions
    {
        /// <summary>
        /// Adds Keycloak client services to the service collection.
        /// </summary>
        /// <param name="services">The service collection to add the Keycloak client services to.</param>
        /// <returns>The service collection with Keycloak client services added.</returns>
        public static IServiceCollection AddKeycloakClient(this IServiceCollection services, Action<KeycloakClientOptions> configureOptions)
        {
            KeycloakClientOptions _keycloakClientOptions = new();

            services.Configure(configureOptions);

            _keycloakClientOptions.Validate();

            services.AddMemoryCache();

            services.AddTransient<AttachAccessTokenHandler>();

            services.AddHttpClient(Constant.KeycloakAccessTokenClient, (serviceProvider, client) =>
            {
                client.BaseAddress = new Uri(_keycloakClientOptions.KeycloakBaseUrl);

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

            services.AddHttpClient(Constant.KeycloakClient, (serviceProvider, client) =>
            {
                client.BaseAddress = new Uri(_keycloakClientOptions.AdminBaseUrl);

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            }).AddHttpMessageHandler<AttachAccessTokenHandler>();

            services.AddScoped<IKeycloakClient, KeycloakClient>();

            return services;
        }


    }
}
