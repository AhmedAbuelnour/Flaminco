# Flaminco.StateMachine

`Flaminco.StateMachine` is a lightweight, flexible library for building and executing state machines in .NET applications. It provides an easy-to-use framework for managing states, transitions, and asynchronous state processing, leveraging .NET's dependency injection and logging capabilities.

---

## Features

- **Keyed State Management**: Define states with unique keys using attributes.
- **Dependency Injection Support**: Easily integrate with the .NET Dependency Injection container.
- **Asynchronous Processing**: Execute state logic asynchronously for modern applications.
- **Snapshot Tracking**: Capture snapshots of state transitions for debugging and audit purposes.
- **Extensibility**: Create custom states and transition logic by implementing abstract methods.

---

## Installation

To use this library in your project, install the NuGet package:

```bash
Install-Package Flaminco.StateMachine
```

---

## Getting Started

### 1. Define Your States

Create state classes by inheriting from the `State<TPayload>` class and annotating them with the `[StateKey]` attribute. Each state must implement the `Handle` methods to define its behavior.

```csharp
[StateKey(nameof(StateA))]
public class StateA(ILogger<StateA> logger) : State<StateObject>(logger)
{
    public override async ValueTask<bool> Handle(StateContext<StateObject> context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("State A");

        context.Payload.Data = "State A";

        context.SetState(nameof(StateB)); 

        return true; // true mean continue to next state, false mean stop
    }

    public override async ValueTask<bool> Handle(StateContext<StateObject> context, Exception exception, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("State A Error");

        return false; // in case of error, stop the state machine
    }
}

[StateKey(nameof(StateB))]
public class StateB(ILogger<StateB> logger) : State<StateObject>(logger)
{
    public override async ValueTask<bool> Handle(StateContext<StateObject> context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("State B");
        context.Payload.Data = "State B";
        return false;
    }

    public override async ValueTask<bool> Handle(StateContext<StateObject> context, Exception exception, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("State B Error");

        context.SetState(nameof(StateA)); // in case of error, go back to StateA

        return true;
    }
}
```

### 2. Define Your Payload

The `TPayload` represents the object being processed by the state machine. Define your payload class as required.

```csharp
public class StateObject
{
    public string Data { get; set; }
}
```

### 3. Configure the State Machine

Register the states and the state machine context in your `Startup.cs` or equivalent:

```csharp
services.AddStateMachine<IStateScanner>();
```

This scans the assembly for all state classes and registers them with the dependency injection container.

### 4. Use the State Machine

Create and use the state machine in your application by initializing the `StateContext` and setting the initial state.

```csharp
public class Example(StateContext<StateObject> stateContext)
{
    public async Task Handle(CancellationToken cancellationToken)
    {
        // Initialize with the initial state and payload
        stateContext.SetState(nameof(StateA), new StateObject
        {
            Data = "Initial data to start with"
        });

        // Process the state machine
        await stateContext.ProcessAsync(cancellationToken);

        // Log state snapshots
        foreach (var snapshot in stateContext.StateSnapshots)
        {
            Console.WriteLine(snapshot);
        }

        return Results.Ok();
    }
}
```

---

## Example Output

When running the above example, the state machine transitions through `StateA` and `StateB`, producing the following output:

```
State A
State B
StateSnapshot { Key = StateA, Previous = null, Current = "State A", Timestamp = 123456789 }
StateSnapshot { Key = StateB, Previous = "State A", Current = "State B", Timestamp = 123456790 }
```

---

## Advanced Features

### State Snapshots

Each state transition generates a `StateSnapshot` that includes:
- **Key**: The state key.
- **Previous**: The previous state value (serialized).
- **Current**: The current state value (serialized).
- **Timestamp**: The timestamp of the transition.

These snapshots are stored in the `StateSnapshots` list of the `StateContext` and can be used for debugging or auditing.

### Exception Handling

Each state can handle exceptions by overriding the `Handle` method that accepts an `Exception` parameter. This allows for custom error handling logic.

---

## Logging

The library uses `Microsoft.Extensions.Logging` to log state transitions and exceptions. Ensure that logging is configured in your application.

---

## License

This project is licensed under the MIT License.

---

## Contributing

Contributions are welcome! Please submit issues and pull requests via the GitHub repository.

---

## Contact

For any questions or issues, feel free to reach out or open an issue on GitHub.
