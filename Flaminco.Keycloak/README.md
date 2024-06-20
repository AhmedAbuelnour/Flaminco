# Flaminco.Keycloak

`Flaminco.Keycloak` is a .NET library that simplifies the integration of Keycloak for authentication and client communication in your ASP.NET Core applications.

## Getting Started

### Installation

To get started with `Flaminco.Keycloak`, you need to configure the Keycloak services in your ASP.NET Core application. You can do this by adding the following line:

```csharp
builder.Services.AddKeycloakJwtBearerAuthentication(builder.Configuration).AddKeycloakClient();
```

### Configuration

Ensure that you have the required Keycloak configuration in your `appsettings.json` file. Below is an example of the necessary Keycloak configuration:

```json
"Keycloak": {
  "AuthUrl": "http://localhost:8080",
  "Realm": "UserTracker-DEV",
  "Audience": "Backend",
  "RequireHttpsMetadata": "false",
  "RoleClaimType": "roles",
  "NameClaimType": "name",
  "ClockSkew": "00:00:15",
  "SaveToken": false,
  "Credentials": {
    "ClientSecret": "HUE80PxMTAQescNU2MKwTpYf7oNPEZ3U",
    "ClientId": "Backend"
  }
}
```

### Usage

#### AddKeycloakJwtBearerAuthentication

The `AddKeycloakJwtBearerAuthentication` method configures JWT bearer authentication using Keycloak. It injects the necessary services to authenticate requests against your Keycloak server.

#### AddKeycloakClient

The `AddKeycloakClient` method injects the Keycloak client services, enabling your application to communicate with the Keycloak server for various operations, such as managing users and groups.

### Example

Here's an example of how to configure and use `Flaminco.Keycloak` in your ASP.NET Core application:

1. **Configure Services**:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddKeycloakJwtBearerAuthentication(builder.Configuration).AddKeycloakClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

2. **Add Keycloak Configuration**:

Add the Keycloak configuration to your `appsettings.json` file:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Keycloak": {
    "AuthUrl": "http://localhost:8080",
    "Realm": "UserTracker-DEV",
    "Audience": "Backend",
    "RequireHttpsMetadata": "false",
    "RoleClaimType": "roles",
    "NameClaimType": "name",
    "ClockSkew": "00:00:15",
    "SaveToken": false,
    "Credentials": {
      "ClientSecret": "HUE80PxMTAQescNU2MKwTpYf7oNPEZ3U",
      "ClientId": "Backend"
    }
  }
}
```

### Additional Features

`Flaminco.Keycloak` provides various additional features for managing Keycloak resources, such as users and groups. Please refer to the library documentation for more details on these features.

### Contributing

Contributions are welcome! If you find any issues or have suggestions for improvements, please open an issue or submit a pull request.

### License

This project is licensed under the MIT License.

---

With this README, users will have a clear understanding of how to set up and use the `Flaminco.Keycloak` library in their ASP.NET Core applications.