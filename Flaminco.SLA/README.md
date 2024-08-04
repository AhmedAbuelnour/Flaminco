# Flaminco.SLA Library

The `Flaminco.SLA` library is a .NET library designed to calculate Service Level Agreement (SLA) related metrics such as delivery dates, remaining days, and working days. It includes features to account for holidays and custom working days.

## Features

- **Calculate Remaining Days**: Determine the number of remaining working days until a delivery date.
- **Calculate Delivery Date**: Calculate the delivery date based on task duration and working days.
- **Check Working Day**: Verify if a specific date is a working day.
- **Check Holiday**: Verify if a specific date is a holiday.
- **Calculate Spent Days**: Calculate the number of spent working days between two dates.

## Installation

```shell
dotnet add package Flaminco.SLA
```

## Usage

Here's an example of how to use the SLAServiceBase class:

```csharp

public class SLAService : SLAServiceBase
{
    public SLAService(ILogger<SLAService> logger) : base(logger) { }

    /// <summary>
    /// Gets the calendar information.
    /// In a real example, you will get the data from the database, and it is preferred to cache it.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the calendar information.</returns>
    public override ValueTask<CalendarInfo> GetCalendar()
    {
        return ValueTask.FromResult(new CalendarInfo
        {
            WorkingDays = new List<DayOfWeek>
            {
                DayOfWeek.Saturday,
                DayOfWeek.Sunday,
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Thursday,
            },
            Holidays = new List<Holiday>
            {
                new Holiday(new DateOnly(2024, 01, 01), new DateOnly(2024, 01, 11))
            }
        });
    }
}

// Inject it to DI to be accessable within your app.

builder.Services.AddScoped<SLAServiceBase,SLAService>();

```


## Contribution
Contributions are welcome! If you have suggestions, bug reports, or contributions, please submit them as issues or pull requests on our GitHub repository.

## License
This project is licensed under the MIT License.