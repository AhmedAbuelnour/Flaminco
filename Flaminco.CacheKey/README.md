# Flaminco.CacheKeys

The `Flaminco.CacheKeys` This library provides a structured way to define cache keys, ensuring they are uniquely
identifiable across different regions and optionally tagged for finer-grained categorization and retrieval.

## Features

- **Region-based Segmentation**: Define cache keys within specific regions to manage data more effectively across
  different contexts or locations.
- **Tagging Capability**: Attach tags to cache keys for additional metadata, aiding in sorting, filtering, and
  identifying cache entries.
- **Implicit String Conversion**: Use cache keys directly as strings for ease of integration with existing caching
  mechanisms.

## Getting Started

### Installation

To install the `Flaminco.CacheKeys` package, use the following command in the .NET CLI:

```shell
dotnet add package Flaminco.CacheKeys
```

Usage

```csharp
 string dummyValue = await hybridCache.GetOrCreateAsync<string>(new CacheKey
 {
     Region = "Lookup", // Required
     Key = "Categories", // Required
     Tags = ["v1", "api"] // Optional
 }, async (x) =>
 {
     return "dummy category";
 }, tags: ["tag1", "tag2"]);

 // incase you need to remove all cached items by tag
 await hybridCache.RemoveTagAsync("v1");
```

## Contribution

Contributions are welcome! If you have suggestions, bug reports, or contributions, please submit them as issues or pull
requests on our GitHub repository.

## License

This project is licensed under the MIT License.