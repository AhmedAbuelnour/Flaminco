# Flaminco.TickCronos

Flaminco.TickCronos is a flexible and lightweight .NET package designed for cron-based background job scheduling. Utilizing the high-performance .NET 8 TimeProvider, TickCronons ensures precise and efficient timing, while allowing you to set unique TimeProvider configurations for each scheduled task.

## Features

- Cron-based scheduling with second-level precision.
- High-performance timing with .NET 8 TimeProvider.
- Flexible time zone support for individual jobs.
- Scoping support for dependency injection within job execution.

## Installation

Install Flaminco.TickCronos via NuGet:

```bash
dotnet add package Flaminco.TickCronos
```

## Getting Started

### Step 1: Create a Cron Job

Inherit from `TickCrononsJobService` to create your job class, implementing the `ExecuteAsync` method where the job logic is defined.

```csharp

public class MyTickCronosExample1 : TickCrononsJobService
{
    public MyTickCronosExample1(ITickCrononsConfig<MyTickCronosExample1> config, IServiceProvider serviceProvider, ILogger<MyTickCronosExample1> logger)
        : base(config.CronExpression, config.TimeProvider, serviceProvider, logger) { }

    public override string CronJobName => nameof(MyTickCronosExample1);

    public override async Task ExecuteAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        // Define your scheduled job logic
        Console.WriteLine("Running scheduled job for MyTickCronosExample1...");
        await Task.Delay(1000, cancellationToken);  // Simulated work
    }
}


public class MyTickCronosExample2(ITickCronosConfig<MyTickCronosExample2> config, IServiceProvider serviceProvider, ILogger<MyTickCronosExample2> logger)
    : TickCronosJobService(config.CronExpression, config.TimeProvider, serviceProvider, logger)
{
    public override string CronJobName => nameof(MyTickCronosExample2);

    public override async Task ExecuteAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        // Your job logic here
        Console.WriteLine("Running scheduled job...");
        await Task.Delay(1000, cancellationToken);  // Simulated work
    }
}
```

### Step 2: Configure and Register Jobs

In your application setup (e.g., `Program.cs`), use `AddTickCronosJob` to configure and add your job with a cron expression and time provider.


```csharp
builder.Services.AddTickCronosJob<MyTickCronosExample1>(a =>
{
    a.CronExpression = "*/5 * * * * *";  // Every 5 seconds
    a.TimeProvider = TimeProvider.System; // default if not provided
});

builder.Services.AddTickCronosJob<MyTickCronosExample2>(a =>
{
    a.CronExpression = "*/5 * * * * *";  // Every 5 seconds
    a.TimeProvider = new EgyptTimeProvider();
});
```

### Custom Time Provider Example

TickCronons allows for custom time providers. Here’s an example of a `EgyptTimeProvider` to run jobs in a specific time zone.

```csharp
public class EgyptTimeProvider : TimeProvider
{
    private readonly TimeZoneInfo _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
    public override DateTimeOffset GetUtcNow()
    {
        return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, _timeZoneInfo);
    }

    public override TimeZoneInfo LocalTimeZone => _timeZoneInfo;
}
```

## Configuration Options

- **CronExpression**: Defines the cron schedule.
- **TimeProvider**: Customize time zones or use the default system time.


## Logging

Flaminco.TickCronos logs job status, execution times, and errors using the .NET `ILogger`. Ensure logging is set up in your application to capture job activities.

## License

This project is licensed under the MIT License.

