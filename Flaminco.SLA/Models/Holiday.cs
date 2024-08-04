namespace Flaminco.SLA.Models
{
    /// <summary>
    /// Represents a holiday with a start date and an end date.
    /// </summary>
    public record struct Holiday(DateOnly StartDate, DateOnly EndDate);
}
