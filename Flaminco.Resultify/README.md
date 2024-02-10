# Flaminco.Resultify

The `Flaminco.Resultify` library provides a robust mechanism for handling operation results and errors within your applications, facilitating clear and consistent error handling and result management. This documentation outlines the usage and features of the `Result` and `Error` classes included in the library.

## Overview

`Result`: Represents the outcome of an operation, encapsulating the success or failure status along with any relevant errors.
`Error`: Models an error with details such as an error code, optional description, and the source line number for easier debugging.

## Getting Started

### Installation

To install the `Flaminco.Resultify` package, use the following command in the .NET CLI:

```shell
dotnet add package Flaminco.Resultify
```

### `Result` Class

The `Result` class is designed to express the outcome of a particular operation in a functional way, allowing for easy checking of success or failure and access to any associated errors.

### Features

**Success and Failure Indication**: Quickly determine whether an operation was successful or not.
**Factory Methods**: Utilize static methods for creating instances representing success or failure.

### Usage

```csharp
// Creating a Success Result

var successResult = Result.Success();
var successValueResult = Result.Success(42); // For operations that return a value

// Creating a Failure Result

var failureResult = Result.Failure(notFoundError);

// Accessing Result Data

if (successValueResult.IsSuccess)
{
    Console.WriteLine($"Operation succeeded with value: {successValueResult.Value}");
}
else
{
    Console.WriteLine($"Operation failed: {successValueResult.Error.Description}");
}

```

### `Error` Class

The `Error` record simplifies the representation and handling of errors, providing essential information that can be used for debugging and user feedback.

### Features

**Error Code and Description**: Quickly determine whether an operation was successful or not.
**Source Line Number**: Access a read-only list of errors associated with a failed operation.
**Implicit Conversion to `Result` **: Directly use an `Error` instance to create a failure Result, streamlining error handling..

### Usage

```csharp
// Defining an error
var notFoundError = new Error("404", "Item not found", ErrorType.NotFound);
```


## Advanced Usage

**Implicit Error to Result Conversion**


```csharp
// Errors can be implicitly converted to Result objects, allowing for concise error handling:

Error validationError = Error.Validation("InvalidInput", "Input value is invalid.");
Result result = validationError; // Implicitly converts to a failure result
```

Handling Result<TValue>

```csharp
// For operations that return a value upon success, Result<TValue> can be used. It inherits from Result, adding support for a value:

var valueResult = Result.Success("Hello, World!");
if (valueResult.IsSuccess)
{
    Console.WriteLine(valueResult.Value); // Outputs: Hello, World!
}
```


## Contribution
Contributions are welcome! If you have suggestions, bug reports, or contributions, please submit them as issues or pull requests on our GitHub repository.

## License
This project is licensed under the MIT License.