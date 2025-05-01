# Flaminco.TickCronos

Flaminco.TickCronos is a flexible and lightweight .NET package designed for cron-based background job scheduling. Utilizing the high-performance .NET 8 TimeProvider, TickCronos ensures precise and efficient timing, while allowing you to set unique TimeProvider configurations for each scheduled task.

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

Inherit from `JobRequest` and override the `Consume` method to implement your job logic:

```csharp
public class TestRequest : JobRequest
{
    public TestRequest(IJobRequestConfig<TestRequest> jobRequestConfig, 
                      JobQueue queue, 
                      ILogger<TestRequest> logger) 
        : base(jobRequestConfig.CronExpression, jobRequestConfig.TimeProvider, queue, logger)
    {
    }

    public override string Name => "TEST_REQUEST";

    public override async Task Consume(CancellationToken cancellationToken)
    {
        // Your job logic here
        Console.WriteLine($"Executing {Name} job at {DateTime.Now}");
        
        // Example: Access any required services through dependency injection
        // await _someService.DoWorkAsync();
    }
}
```

### Step 2: Configure and Register Jobs with Different Time Zones

In your application setup (e.g., `Program.cs`), you can register multiple jobs with different time providers:

```csharp
// Add job queue first
var jobQueueBuilder = builder.Services.AddJobQueue();

// Register a job with System TimeProvider (local system time zone)
jobQueueBuilder.AddJob<SystemTimeJob>(options =>
{
    options.CronExpression = "0 */30 * * * *"; // Every 30 minutes
    options.TimeProvider = TimeProvider.System; // Uses local system time zone
});

// Register a job with UTC TimeProvider
jobQueueBuilder.AddJob<UtcTimeJob>(options =>
{
    options.CronExpression = "0 0 */1 * * *"; // Every hour
    options.TimeProvider = UtcTimeProvider.System; // Uses UTC time zone
});

// Register a job with Saudi TimeProvider
jobQueueBuilder.AddJob<SaudiTimeJob>(options =>
{
    options.CronExpression = "0 0 9 * * *"; // Every day at 9 AM Saudi time
    options.TimeProvider = SaudiTimeProvider.System; // Uses Saudi Arabia time zone
});

// Register a job with custom scheduling logic by overriding ConfigureNextOccurrence
jobQueueBuilder.AddJob<CustomScheduleJob>(options =>
{
    // CronExpression is set to null to use custom scheduling logic
    options.CronExpression = null;
    options.TimeProvider = TimeProvider.System;
});
```

With this configuration:
- `SystemTimeJob` runs every 30 minutes in your local system time zone
- `UtcTimeJob` runs every hour in UTC time zone
- `SaudiTimeJob` runs daily at 9 AM Saudi time
- `CustomScheduleJob` uses custom scheduling logic defined in the job class

### Custom Scheduling Logic

You can create a job with custom scheduling logic by overriding the `ConfigureNextOccurrence` method:

```csharp
public class CustomScheduleJob : JobRequest
{
    private readonly int _intervalMinutes;
    
    public CustomScheduleJob(IJobRequestConfig<CustomScheduleJob> jobRequestConfig,
                          JobQueue queue,
                          ILogger<CustomScheduleJob> logger)
        : base(jobRequestConfig.CronExpression, jobRequestConfig.TimeProvider, queue, logger)
    {
        // Custom interval in minutes
        _intervalMinutes = 15;
    }

    public override string Name => "CUSTOM_SCHEDULE_JOB";

    // Override ConfigureNextOccurrence to implement custom scheduling logic
    public override ValueTask<DateTimeOffset?> ConfigureNextOccurrence(CancellationToken cancellationToken = default)
    {
        // Example: Run every X minutes from the current time
        var nextOccurrence = TimeProvider.GetLocalNow().AddMinutes(_intervalMinutes);
        
        return ValueTask.FromResult<DateTimeOffset?>(nextOccurrence);
    }

    public override async Task Consume(CancellationToken cancellationToken)
    {
        // Your job logic here
        Console.WriteLine($"Executing {Name} with custom scheduling at {DateTime.Now}");
        
        // Example: Access any required services through dependency injection
        await Task.CompletedTask;
    }
}
```

This job will run every 15 minutes from the current time, regardless of when it was started.

## Configuration Options

- **CronExpression**: Defines the cron schedule. Set to null to use custom scheduling logic.
- **TimeProvider**: Customize time zones or use the default system time.

## Logging

Flaminco.TickCronos logs job status, execution times, and errors using the .NET `ILogger`. Ensure logging is set up in your application to capture job activities.

## License

This project is licensed under the MIT License.

