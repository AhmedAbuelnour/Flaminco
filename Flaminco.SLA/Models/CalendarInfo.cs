namespace Flaminco.SLA.Models;

/// <summary>
///     Represents the calendar information including holidays and working days.
/// </summary>
public class CalendarInfo
{
    /// <summary>
    ///     Gets or sets the list of holidays.
    /// </summary>
    public required IEnumerable<Holiday> Holidays { get; set; }

    /// <summary>
    ///     Gets or sets the list of working days.
    /// </summary>
    public required IEnumerable<DayOfWeek> WorkingDays { get; set; }
}