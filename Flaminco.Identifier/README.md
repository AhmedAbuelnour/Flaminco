# Flaminco.Identifier

The `Flaminco.Identifier` library offers a robust, type-safe mechanism for handling entity identifiers in .NET
applications, using GUIDs to ensure uniqueness while preventing the accidental interchange of identifiers between
different entity types.

## Features

- **Type Safety**: Guarantees that identifiers are specific to their entity types, eliminating the risk of mixing up
  identifiers between entities.
- **GUID Utilization**: Leverages globally unique identifiers (GUIDs) to ensure the uniqueness of each identifier.
- **Ease of Use**: Provides a straightforward approach to generating and managing entity-specific identifiers with
  support for implicit conversion to and from `Guid`, simplifying integration with existing systems.

## Getting Started

### Installation

To install the `Flaminco.Identifier` package, use the following command in the .NET CLI:

```shell
dotnet add package Flaminco.Identifier
```

Usage

Define an Identifier for Your Entity

```csharp
public class User 
{
    public Identifier<User> Id { get; set; }
}
```

## Best Practices

Utilize Identifier<TEntity> wherever you need a unique identifier for an entity, to leverage type safety and avoid the
pitfalls of using raw GUIDs or other primitive types.
When designing APIs or services, prefer accepting and returning Identifier<TEntity> to enforce type safety across your
application's boundaries.

## Contribution

Contributions are welcome! If you have suggestions, bug reports, or contributions, please submit them as issues or pull
requests on our GitHub repository.

## License

This project is licensed under the MIT License.