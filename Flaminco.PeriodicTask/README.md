# Flaminco.PeriodicTask

The `Flaminco.PeriodicTask` namespace contains the `PeriodicBackgroundService`, an abstract class designed to simplify the creation of background services that execute tasks at regular intervals. This framework is built upon .NET's BackgroundService and provides a structured way to implement periodic tasks such as cleanup operations, data processing, or any scheduled background work.

## Features

- **Scheduled Execution**: Automatically schedules tasks based on a specified period.
- **Enable/Disable Control**: Allows dynamic enabling or disabling of the service.
- **Logging**: Integrates with Microsoft's logging infrastructure to log service activity.
- **Graceful Cancellation**: Supports cancellation tokens for gracefully stopping tasks.

## Getting Started

### Installation

To install the `Flaminco.PeriodicTask` package, use the following command in the .NET CLI:

```shell
dotnet add package Flaminco.PeriodicTask
```

Usage

Define your background service.

```csharp
public class RemoveLogItemsBackgroundService : PeriodicBackgroundService
{
    public RemoveLogItemsBackgroundService(ILogger<RemoveLogItemsBackgroundService> logger) : base(logger)
    {
    }

    protected override string Name => "RemoveLogItems";
    protected override bool IsEnabled { get; set; } = true;
    protected override TimeSpan Period => TimeSpan.FromDays(30);
    protected override DateTime? LastRunOn { get; set; }

    protected override Task RunAsync(CancellationToken stoppingToken)
    {
        // Implementation logic for removing log items
        return Task.CompletedTask;
    }
}

```

Configure the Service

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHostedService<RemoveLogItemsBackgroundService>();
}
```

## Service Configuration

Configure the service properties (`IsEnabled`, `Period`, `LastRunOn`) to control its behavior. These can be set programmatically or loaded from an application's configuration (e.g., `appsettings.json`).

## Contribution
Contributions are welcome! If you have suggestions, bug reports, or contributions, please submit them as issues or pull requests on our GitHub repository.

## License
This project is licensed under the MIT License.