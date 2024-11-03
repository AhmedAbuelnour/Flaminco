# Flaminco.Migration

`Flaminco.Migration` is designed to simplify the process of executing database migration scripts embedded within your
.NET assemblies built using (DbUP). By leveraging this library, you can ensure that your database schema is up-to-date
with the latest changes whenever your application starts. This library supports the use of directories to organize and
prioritize the execution order of your scripts.

## Features

* Execute database migration scripts embedded in assemblies.
* Supports organizing scripts into directories with prioritized execution order.
* Integrated with IHostedService to ensure migrations are run before the application starts accepting requests.
* Leverage `appsettings.json` for configuration.
* Supports for always executing migration scripts.

## Installation

```shell
dotnet add package Flaminco.Migration
```

## Getting Started

### Installation

#### Step 1: Add Migration to Your Services

To get started with `Flaminco.Migration`, you need to configure the Migration services in your ASP.NET Core application.
You can do this by adding the following line:

Using Direct Configuration

```csharp

builder.Services.AddMigration<Program>(options =>
{
    options.ConnectionString = "Server=localhost;****";
    options.Directories = ["WebApplication1.Scripts.Tables","WebApplication1.Scripts.StoredProcedures"];
    options.AlwaysExexuteDirectories = ["WebApplication1.Scripts.StoredProcedures"]; // means run the scripts inside these directories each time the upgrader run.
});
```

Using Configuration from `appsettings.json`

```json
{
  "Migration": {
    "ConnectionString": "Server=localhost;****",
    "Directories": [
      "WebApplication1.Scripts.Tables",
      "WebApplication1.Scripts.StoredProcedures"
    ],
    "AlwaysExecuteDirectories": [
      "WebApplication1.Scripts.StoredProcedures"
    ]
  }
}
```

Modify your Program.cs or Startup.cs to use the configuration:

```csharp
builder.Services.AddMigration<Program>(builder.Configuration);
```

#### Step 2: Embed Your SQL Scripts

Ensure your SQL scripts are embedded as resources in your project. Modify your .csproj file to include the scripts:

```xml
<ItemGroup>
    <EmbeddedResource Include="Scripts\Tables\**\*.sql" />
    <EmbeddedResource Include="Scripts\StoredProcedures\**\*.sql" />
</ItemGroup>
```

#### Step 3: Ensure the Correct Execution Order

The order of directories specified in the Directories array will determine the execution order of the scripts. In the
above example, scripts in the Tables directory will be executed before those in the StoredProcedures directory. If no
directories are specified, all scripts in the assembly will be loaded and executed by the default DbUp ordering
mechanism.

### Contributing

Contributions are welcome! If you find any issues or have suggestions for improvements, please open an issue or submit a
pull request.

### License

This project is licensed under the MIT License.
