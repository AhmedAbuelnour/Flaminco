namespace Flaminco.PeriodicAction;

public class PeriodicActionOption
{
    public TimeSpan TimeInterval { get; set; } = TimeSpan.Zero;
    public Action PeridicAction { get; set; }
}
