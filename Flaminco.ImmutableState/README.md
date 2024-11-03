# Flaminco.ImmutableStates

`Flaminco.ImmutableStates` is a .NET library designed to manage immutable state entities within an application. It
provides a simple and efficient way to look up and manage immutable states using Entity Framework Core.

## Features

- **Asynchronous Operations**: All lookups are performed asynchronously, ensuring non-blocking I/O operations.
- **Flexible Queries**: Supports querying by ID, code, group, or custom predicates.
- **Extensible**: Easily extendable to support additional functionality.
- **Dependency Injection Support**: Seamlessly integrates with ASP.NET Core's dependency injection system.

## Installation

You can install the `Flaminco.ImmutableStates` library via NuGet:

```bash
dotnet add package Flaminco.ImmutableStates
```

## Getting Started

### 1. Define Your DbContext

Ensure that your `DbContext` includes a `DbSet<ImmutableState>` for immutable states:

```csharp
public class YourDbContext : DbContext
{
    public DbSet<ImmutableState> ImmutableStates { get; set; }
}
```

### 2. Register the Immutable State Lookup Service

In your `Startup.cs` or `Program.cs`, register the `IImmutableStateLookup` service:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddDbContext<YourDbContext>(options => 
        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
        
    services.AddImmutableState<YourDbContext>();
}
```

### 3. Use the Immutable State Lookup

Inject the `IImmutableStateLookup` into your services or controllers:

```csharp
public class YourService
{
    private readonly IImmutableStateLookup _immutableStateLookup;

    public YourService(IImmutableStateLookup immutableStateLookup)
    {
        _immutableStateLookup = immutableStateLookup;
    }

    public async Task<ImmutableState?> GetStateByCodeAsync(int code)
    {
        return await _immutableStateLookup.GetByCodeAsync(code);
    }
}
```

## Contributing

Contributions are welcome! If you have suggestions, bug reports, or contributions, please submit them as issues or pull
requests on our GitHub repository.

## License

This project is licensed under the MIT License.