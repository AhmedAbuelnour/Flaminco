# Flaminco.ImmutableLookups

The `Flaminco.ImmutableLookups` package is designed to simplify the querying of immutable lookup entities in a database context. It leverages Entity Framework Core to provide efficient and type-safe data access.

## Table of Contents

- [Setup](#setup)
- [Defining Entities](#defining-entities)
- [Getting Started](#getting-started)
- [Contribution](#contribution)
- [License](#license)

## Setup

- Ensure your project is set up to use Entity Framework Core.
- Add the `Flaminco.ImmutableLookups` package to your project.

## Defining Entities

Define your immutable lookup entities by inheriting from `ImmutableLookupEntityBase<TKey>`:

```csharp
using Flaminco.ImmutableLookups.Abstractions;
using System.Collections.Generic;

public record WorkflowStatus(int Id, int Code, string Module, Dictionary<string, string> Description) : ImmutableLookupEntityBase<int>(Id, Code, Module);
```

## Getting Started

Create your DbContext:

```csharp
using Microsoft.EntityFrameworkCore;

public class MyDbContext : DbContext
{
    public DbSet<WorkflowStatus> WorkflowStatuses { get; set; }
    
    // Define your DbContext configuration here
}
```

Register the IImmutableLookupQuery service in your Dependency Injection (DI) container

```csharp
using Flaminco.ImmutableLookups.Implementations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IImmutableLookupQuery<WorkflowStatus, int>, ImmutableLookupQuery<MyDbContext, WorkflowStatus, int>>();

var app = builder.Build();

```

Use the IImmutableLookupQuery in your application:

```csharp
public class Example
{
    private readonly IImmutableLookupQuery<WorkflowStatus, int> _lookupQuery;

    public Example(IImmutableLookupQuery<WorkflowStatus, int> lookupQuery)
    {
        _lookupQuery = lookupQuery;
    }

    public async Task GetLookups()
    {
        var lookups = await _lookupQuery.GetByModuleAsync("module_name");
        // Process lookups as needed
    }
}

```

## Contribution
Contributions are welcome! If you have suggestions, bug reports, or contributions, please submit them as issues or pull requests on our GitHub repository.

## License
This project is licensed under the MIT License.